#include <exception>
#include <iostream>
#include <fstream>
#include <cctype>
#include <future>
#include <sstream>

#include <boost/beast/core.hpp>
#include <boost/beast/websocket.hpp>
#include <boost/asio/connect.hpp>
#include <boost/asio/ip/tcp.hpp>

#include "../third_party/geometric.h"
#include "../third_party/mesh.h"
#include "../third_party/glwin.h"
#include "../third_party/misc.h"
#include "../third_party/misc_gl.h"
#include "../third_party/cnn.h"
#include "../include/misc_image.h"
#include "../include/physmodel.h"
#include "../include/handtrack.h"

using tcp = boost::asio::ip::tcp;
namespace websocket = boost::beast::websocket;

int f_shutdown;

void shutdown_handler(int x)
{
    f_shutdown = 1;
}

std::vector<std::vector<Pose>> LoadAnimBank(std::string filename, size_t pose_array_size)
{
	std::vector<std::vector<Pose>> animbank;
	std::ifstream pfile(filename);
	if (!pfile.is_open())
		throw "unable to open animation bank file";
	std::string line;
	while (std::getline(pfile, line) && line != "")
	{
		std::vector<Pose> pose(pose_array_size);
		std::stringstream linestream(line);
		for (auto &p : pose)
			linestream >> p;
		animbank.push_back(pose);
	}
	return animbank;
}

float4x4 GetSensorPerspectiveMatrix(DCamera &dcam, float2 range)  // range is clipnear,clipfar
{
	float4x4 p;   // this is currently ignoring the principal point
	p.x.x = dcam.focal().x * 2 / dcam.dim().x;
	p.y.y = dcam.focal().y * 2 / dcam.dim().y;
	p.z.z = (range[1] + range[0]) / (range[0] - range[1]);
	p.z.w = -1;
	p.w.z = (2 * range[1] * range[0]) / (range[0] - range[1]);
	return p;
}

Image<unsigned short> FakeDepth(PhysModel &model, const DCamera &dcam)  // software version raster of depth buffer for debugging synthetic depth data
{
	Image<unsigned short> depth(dcam);
	depth.cam.depth_scale = dcam.depth_scale;
	for (auto p : rect_iteration(depth.dim()))
		depth.pixel(p) = (unsigned short)(model.HitCheck({ 0,0,0 }, depth.cam.deprojectz(float2(p), 4.0f)).impact.z / depth.cam.depth_scale);
	return depth;
}

template<class T> inline std::vector<T> & FlipY(std::vector<T> &image, int2 dim)  // 
{
	for (int y = 0; y < dim.y / 2; y++)	for (int x = 0; x < dim.x; x++)
		std::swap(image[y*dim.x + x], image[(dim.y - 1 - y)*dim.x + x]);
	return image;
}

int main(int argc, const char **argv) try
{
	signal(SIGINT, shutdown_handler);
	f_shutdown = 0;
	
	// Set up the HTTP Socket Client
    auto const host = "18.220.146.229";
    auto const port = "3001";
    
    boost::asio::io_service ios;
    websocket::stream<tcp::socket> ws{ios};

    tcp::resolver resolver(ios);
    tcp::resolver::query query(host, port);
    auto const lookup = resolver.resolve(query);

    boost::asio::connect(ws.next_layer(), lookup);
    ws.handshake(host, "/");
    
    ws.write(boost::asio::buffer(std::string("CAMERA:: CONNECTED")));
	
	std::cout << "testing the hand tracking system using synthetically generated depth data.\n";
	std::string afilename = argc >= 2 ? argv[1] : "../assets/animbank.pose";

	HandTracker htk;
	htk.always_take_cnn = 0;
	htk.microforce = 3.0f;   // since a lot of frames were dropped in the animation file, we increase the fitting strength to compensate
	htk.mainthreadpasses = 3;
	PhysModel fakehand = LoadHandModel();
	for (auto &m : fakehand.sdmeshes)
		m.hack = linalg::lerp(m.hack,float4(1),0.75f);  // reduce the hue on the fingers for visualization purposes

	DCamera dcam({ 320,240 }, { 305,305 }, { 160,120 }, 0.001f);  // depth_scale = 0.001f;  // ivycam 0.000124987f;
	float2 drange = { 0.05f, 0.85f }; // needs to be wider than what the segmentation uses  
									  //auto animbank = LoadAnimBank("../hand_db_ivycam/hand_dirp_ivycam_2.pose", handmodel.rigidbodies.size());
	auto animbank = LoadAnimBank(afilename, fakehand.rigidbodies.size());

	GLWin glwin("synthetic-hand-tracker   demonstrating hand pose estimation with synthetically generated depth data", 320+320+100*2+4, 900);  // should fit 1080 allowing window's borders, taskbar 

	bool   software_rasterizer = false ;   // for troubleshooting if opengl depth readback is ever an issue 
	int    animate   = 1;
	float  viewdist  = 0.40f;  // distance opengl viewpoint away from area of focus   use mousewheel to zoom in and out
	float  yaw       = 180.0f; // to spin the view for only the final hand tracking result rendering 
	int    frameid   = 0;

	htk.load_config("../config.json");

	auto drawimagevp = [&](const Image<byte3> &image, int2 c, int2 d, std::string caption = "")
	{
		glViewport(c.x, c.y, d.x, d.y);
		drawimage(image, float2(0, 1.0f), float2(1.0f, -1.0f));   // note the flip on vertical 
		int k=0;
		if(caption.length()) for(auto s:split(caption,"\n")) glwin.PrintString({ 0,k++ }, s.c_str());
	};
	glwin.keyboardfunc = [&](unsigned char key, int x, int y)->void
	{
		switch (std::tolower(key))
		{
		case 'q': case 27:  exit(0); break;  // ESC
		default:
			std::cout << "unassigned key (" << (int)key << "): '" << key << "'" << std::endl;
			break;
		}
	};

	while (glwin.WindowUp())
	{
		if (f_shutdown)
        {
            std::cout << "SHUTTING DOWN GRACEFULLY..." << std::endl;
            ws.close(websocket::close_code::normal);
        }
		
		fakehand.SetPose(animbank[frameid%animbank.size()]);        // assign the pose of our fake hand to the next animation frame
		if(glwin.MouseState && glwin.mousepos.y<glwin.res.y-70)     // leave a bit of space for widgets at the bottom of the window
			yaw += (glwin.mousepos - glwin.mousepos_previous).x ;   // yaw and viewdist used for opengl viewing only, not to be confused with the synthetic depth camera 
		viewdist *= powf(1.1f, (float)glwin.mousewheel);
		auto glcamera = Pose({ 0,0,0.5f }, qmul(float4(0, 0, 1, 0), QuatFromAxisAngle({ 0,1,0 }, yaw*3.14f / 180.0f)))*Pose({ 0,0,viewdist }, { 0,0,0,1 });

		glPushAttrib(GL_ALL_ATTRIB_BITS);
		glViewport(0, 0, glwin.res.x, glwin.res.y);
		glClearColor(0.0f, 1.0f, 1.0f, 1);
		glClear(GL_COLOR_BUFFER_BIT | GL_DEPTH_BUFFER_BIT);

		// render our dummy hand model to a 320x240 section of the gl window
		glViewport(0, glwin.res.y-240, 320, 240);
		glEnable(GL_SCISSOR_TEST);
		glScissor(0, glwin.res.y - 240, 320, 240);
		{
			glClearColor(0.0f, 0.1f, 0.2f, 1);
			glClear(GL_COLOR_BUFFER_BIT | GL_DEPTH_BUFFER_BIT);
			render_scene scene({ { 0,0,0 },{ 1,0,0,0 } });
			glMatrixMode(GL_PROJECTION); glLoadIdentity(); glMultMatrixf(GetSensorPerspectiveMatrix(dcam, drange));
			glMatrixMode(GL_MODELVIEW);	glLoadIdentity(); glRotatef(180, 1, 0, 0);// gluLookAt(0, 0, 0, 0, 0, 1, 0, -1, 0);
			glEnable(GL_COLOR_MATERIAL);
			glColor3f(1, 1, 1);
			glEnable(GL_LIGHTING);
			glEnable(GL_LIGHT0);
			for (auto m : fakehand.GetMeshes(true))   // using the bones subd model for rendering 
				MeshDraw(m);
			glColor3f(1, 1, 0.5f);
			glwin.PrintString({ 0,0 }, "rendering of animated fake hand");  // printed strings will not write to the depth
			glFlush();
		}
		glDisable(GL_SCISSOR_TEST);
		// read back from the opengl depth buffer on the gpu
		Image<float> gpudepth(dcam);  // the depth buffer image floating point 
		glReadPixels(0, glwin.res.y - 240, 320, 240, GL_DEPTH_COMPONENT, GL_FLOAT, gpudepth.raster.data());
		gpudepth = Transform(gpudepth, [drange](float d) {return (drange[0] * drange[1]) / (drange[1] - d * (drange[1] - drange[0]));});  // to depth 
		gpudepth = Transform(gpudepth, [drange](float d) {return 1.0f - (d - drange[0]) / (drange[1] - drange[0]);});  // to [0,1]
		FlipY(gpudepth.raster, gpudepth.dim());

		// use the opengl depth data our system input that uses unsigned short librealsense image format
		auto dimage = (software_rasterizer) ? FakeDepth(fakehand, dcam) : Transform(gpudepth, [dcam, drange](float d)->unsigned short {return static_cast<unsigned short>(((1.0f - d)*(drange[1] - drange[0]) + drange[0]) / dcam.depth_scale);});

		// in the rightmost column visualize ground truth labels based on the fake hand - what we hope the cnn will output
		auto segment = HandSegmentVR(dimage);  // we do an extra segmentation here only to provide background when visualizing the fakehand's landmarks 

		// Call the hand tracking system to update the hand pose based only on the synthetic depth data that was generated
		// hand tracker does its cnn updates in a background thread as it is able, and incremental updates each time in main thread
		htk.update(std::move(dimage));   

		// in the other column toward the right show heatmap results outputted from the cnn
		auto landmark_outputs = VisualizeHMaps(htk.cnn_output_analysis.hmaps, htk.cnn_input);
		auto angle_outputs = ToRGB(UpSample(UpSample(UpSample(ToGrayScale(htk.cnn_output_analysis.vmap)))));
		
		// show the reconstructed hand pose in the bottom view in the window, user can rotate this view with mouse.
		glEnable(GL_SCISSOR_TEST);
		glViewport(1, 1, glwin.res.x - 100*2 - 1, glwin.res.y - 275 - 240 - 1);
		glScissor (1, 1, glwin.res.x - 100*2 - 1, glwin.res.y - 275 - 240 - 1);
		{
			glClearColor(0.0f, 0.1f, 0.2f, 1);
			glClear(GL_COLOR_BUFFER_BIT | GL_DEPTH_BUFFER_BIT);
			render_scene scene(glcamera, Addresses(htk.handmodel.GetMeshes(true)), {}, ColorVerts(Transform(PointCloud(segment, { 0.2f,htk.drangey } ), [&](float3 v) {return segment.cam.pose*v;}), float3(0, 1, 1)));

			ws.write(boost::asio::buffer(std::to_string(htk.handmodel.getPalmLocation().x) + ", " + std::to_string(htk.handmodel.getPalmLocation().y)));

			glLineWidth(2.0f);
			glColor3fv({ 0.50f, 0.50f, 0.60f });
			if (yaw != 180.0f)
				glwirefrustumz(dimage.cam.deprojectextents(), { 0.15f,htk.drangey });  // shows the view volume cone of the virtual depth camera used to generate the synthetic depth
			glColor3f(1, 1, 0.5f);
			glwin.PrintString({ 0,0 }, " Resulting reconstructed hand pose from hand tracking system:");
			glwin.PrintString({ 0,1 }, " use cnn results %s  [c] to toggle ", htk.always_take_cnn ? "always" : "only when leads to better fit");
			if (animate!=1)
				glwin.PrintString({ 0,3 }, "(a)nimate %s  speed=%d", (animate) ? "" : "paused",animate);
		}
		glDisable(GL_SCISSOR_TEST);
		glPopAttrib();
		WidgetSwitch(int2{ 3, 3 }, int2{ 200,30 }, glwin, htk.always_take_cnn , "[c] cnn always").Draw();
		WidgetSlider(int2{ 3,33 }, int2{ 200,30 }, glwin, animate, int2{ 0,4 }, "[a/f] animate ").Draw();

		glwin.SwapBuffers();

		frameid += animate;
	}
	return 0;
}
catch (const char *c)
{
	std::cerr << c << "\n";
	MessageBoxA(GetActiveWindow(), c, "FAIL", 0); //throw(std::exception(c));
}
catch (const std::exception & e)
{
	std::cerr << e.what() << "\n";
	MessageBoxA(GetActiveWindow(), e.what(), "FAIL", 0);
}