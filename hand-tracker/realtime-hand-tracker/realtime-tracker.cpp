#include <cctype>

#include <boost/beast/core.hpp>
#include <boost/beast/websocket.hpp>
#include <boost/asio/connect.hpp>
#include <boost/asio/ip/tcp.hpp>

#include "../third_party/geometric.h"
#include "../third_party/mesh.h"
#include "../third_party/glwin.h"
#include "../third_party/misc_gl.h"
#include "../third_party/json.h"
#include "../include/handtrack.h"
#include "../include/dcam.h"

using tcp = boost::asio::ip::tcp;
namespace websocket = boost::beast::websocket;

auto const HOST = "18.220.146.229";
auto const PORT = "3001";

int f_shutdown;

void shutdown_handler(int x)
{
    f_shutdown = 1;
}

websocket::stream<tcp::socket> setup_socket()
{
	boost::asio::io_service ios;
    websocket::stream<tcp::socket> ws{ios};

    tcp::resolver resolver(ios);
    tcp::resolver::query query(HOST, PORT);
    auto const lookup = resolver.resolve(query);

    boost::asio::connect(ws.next_layer(), lookup);
    ws.handshake(HOST, "/");

	return ws;
}

int main(int argc, char *argv[]) try
{
	RSCam dcam;
	HandTracker htk;

	// First, create a signal handler for graceful shutdowns
	signal(SIGINT, shutdown_handler);
	f_shutdown = 0;

	// Set up the HTTP Socket Client
    websocket::stream<tcp::socket> ws = setup_socket();
    ws.write(boost::asio::buffer(std::string("CAMERA:: CONNECTED")));
	
	GLWin glwin("Hand Tracker", 720, 480);
	dcam.Init("");

	glwin.keyboardfunc = [&](int key, int, int)
	{
		switch (std::tolower(key))
		{
			case 'q':
			case 27:
				dcam.shutdown();
				ws.close(websocket::close_code::normal);
				exit(0);
				break;
			default:
				std::cerr << "unused key " << (char)key << std::endl;
				break;
		}
	};
	htk.load_config( "../config.json");

	while (glwin.WindowUp())
	{
		if (f_shutdown)
        {
            std::cout << "SHUTTING DOWN GRACEFULLY..." << std::endl;
			dcam.shutdown();
            ws.close(websocket::close_code::normal);
        }

		auto dimage = dcam.GetDepth();
		auto dmesh  = DepthMesh(dimage, { 0.1f, 0.7f }, 0.015f, 2);
		auto dxmesh = MeshSmoothish(dmesh.first, dmesh.second);

		for (auto &v : dxmesh.verts)
			v.texcoord = dimage.cam.projectz(v.position) / float2(dimage.cam.dim());

		htk.update(std::move(dimage));

		glPushAttrib(GL_ALL_ATTRIB_BITS);
		glViewport(0, 0, glwin.res.x, glwin.res.y);
		glClearColor(0.1f + 0.2f * (htk.initializing == 50), 0.1f + 0.1f * (htk.initializing > 0), 0.15f, 1);
		glClear(GL_COLOR_BUFFER_BIT | GL_DEPTH_BUFFER_BIT);
		{
			auto allmeshes = Addresses(htk.handmodel.sdmeshes);
			allmeshes.push_back(&dxmesh);
			ws.write(boost::asio::buffer(std::to_string(htk.handmodel.getPalmLocation().x) + ", " + std::to_string(htk.handmodel.getPalmLocation().y)));
			render_scene({ { 0, 0, -0.05f }, normalize(float4(1, 0, 0, 0)) }, allmeshes);  
		}
		glPopAttrib();
		glwin.SwapBuffers();
	}
}
catch (std::exception e)
{
	std::exception_ptr p = std::current_exception();
	try {
		std::rethrow_exception(p);
	} catch(const std::exception& e) {
		std::cout << "REALTIME TRACKER:: Caught exception \"" << e.what() << "\"\n";
	}
}
