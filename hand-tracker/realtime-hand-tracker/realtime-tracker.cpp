#include <cctype>

#include "../third_party/geometric.h"
#include "../third_party/mesh.h"
#include "../third_party/glwin.h"
#include "../third_party/misc_gl.h"
#include "../third_party/json.h"
#include "../include/handtrack.h"
#include "../include/dcam.h"

int main(int argc, char *argv[]) try
{
	GLWin glwin("htk - testing hand tracking system  using realsense depth camera input",1280,720);
	RSCam dcam;
	dcam.Init(argc == 2 ? argv[1] : "");
	HandTracker htk;
	htk.always_take_cnn = false;
	glwin.keyboardfunc = [&](int key, int, int)
	{
		switch (std::tolower(key))
		{
			case 'q': case 27:  exit(0); break;  // ESC
			default: std::cerr << "unused key " << (char)key << std::endl; break;
		}
	};
	htk.load_config( "../config.json");

	while (glwin.WindowUp())
	{
		auto dimage     = dcam.GetDepth();
		auto dimage_rgb = Transform(dimage, [&](unsigned short d) {
			return byte3((unsigned char)clamp((int)(256 * (1.0f - (d*dimage.cam.depth_scale ) / htk.drangey)), 0, 255)); }
		);
		auto ccolor = Image<byte3>({ dcam.dim() });
		if(dcam.dev) ccolor = Image<byte3>({ dcam.dim() }, (const byte3*)dcam.dev->get_frame_data(rs::stream::color_aligned_to_depth));

		auto dmesh  = DepthMesh(dimage, { 0.1f,0.7f }, 0.015f, 2);
		auto dxmesh = MeshSmoothish(dmesh.first, dmesh.second);
		for (auto &v : dxmesh.verts)
			v.texcoord = dimage.cam.projectz(v.position) / float2(dimage.cam.dim());
		dxmesh.pose.position.x = 0.15f;

		htk.update(std::move(dimage));

		glPushAttrib(GL_ALL_ATTRIB_BITS);
		glViewport(0, 0, glwin.res.x, glwin.res.y);
		glClearColor(0.1f + 0.2f * (htk.initializing == 50) , 0.1f + 0.1f * (htk.initializing > 0), 0.15f, 1);
		glClear(GL_COLOR_BUFFER_BIT | GL_DEPTH_BUFFER_BIT);
		{
			auto allmeshes = Addresses(htk.handmodel.sdmeshes);
			allmeshes.push_back(&dxmesh);

			std::cout << htk.handmodel.getPalmLocation() << std::endl;

			render_scene({ { 0, 0, -0.05f }, normalize(float4(1, 0, 0, 0)) }, allmeshes);  
			drawimage(dimage_rgb              , { 0.01f,0.21f }, { 0.2f , -0.2f });
			drawimage(ccolor                  , { 0.01f,0.42f }, { 0.2f , -0.2f });
			drawimage(htk.get_cnn_difference(), { 0.01f,0.63f }, { 0.15f, -0.2f });
		}
		glPopAttrib();
		glwin.SwapBuffers();
	}
}
catch (const char *c)
{
	MessageBoxA(GetActiveWindow(), "FAIL", c, 0);
}
catch (std::exception e)
{
	std::exception_ptr p = std::current_exception();
	try {
		std::rethrow_exception(p);
	} catch(const std::exception& e) {
		std::cout << "REALTIME TRACKER:: Caught exception \"" << e.what() << "\"\n";
	}
	MessageBoxA(GetActiveWindow(), "FAIL", e.what(), 0);
}
