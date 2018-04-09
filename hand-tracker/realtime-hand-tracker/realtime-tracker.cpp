#include <cctype>
#include <signal.h>
#include <sys/time.h>

#include <boost/beast/core.hpp>
#include <boost/beast/websocket.hpp>
#include <boost/asio/connect.hpp>
#include <boost/asio/ip/tcp.hpp>
#include <boost/property_tree/ptree.hpp>
#include <boost/property_tree/json_parser.hpp>

#include "../third_party/geometric.h"
#include "../third_party/mesh.h"
#include "../third_party/glwin.h"
#include "../third_party/misc_gl.h"
#include "../third_party/json.h"
#include "../include/handtrack.h"
#include "../include/dcam.h"

using tcp = boost::asio::ip::tcp;
namespace websocket = boost::beast::websocket;
namespace pt = boost::property_tree;

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
	bool run = true;

	std::string const boneNames[] = {
		"wrist", "palm",
		"thumb_1", "thumb_2", "thumb_3",
		"index_1", "index_2", "index_3",
		"middle_1", "middle_2", "middle_3",
		"third_1", "third_2", "third_3",
		"pinky_1", "pinky_2", "pinky_3", 
	};

	bool sendMeshDataFlag = true;

	// First, create a signal handler for graceful shutdowns
	signal(SIGINT, shutdown_handler);
	f_shutdown = 0;

	// Set up the HTTP Socket Client
    websocket::stream<tcp::socket> ws = setup_socket();
    ws.write(boost::asio::buffer(std::string("CAMERA:: CONNECTED")));

	dcam.Init("");

	htk.load_config( "../config.json");

	while (run)
	{
		if (f_shutdown)
        {
            std::cout << "SHUTTING DOWN GRACEFULLY..." << std::endl;
			dcam.shutdown();
            ws.close(websocket::close_code::normal);
			run = false;
			break;
        }

		auto dimage = dcam.GetDepth();
		auto dmesh  = DepthMesh(dimage, { 0.1f, 0.7f }, 0.015f, 2);
		auto dxmesh = MeshSmoothish(dmesh.first, dmesh.second);

		for (auto &v : dxmesh.verts)
			v.texcoord = dimage.cam.projectz(v.position) / float2(dimage.cam.dim());

		htk.update(std::move(dimage));

		pt::ptree fingers;

		if (sendMeshDataFlag) {
			std::vector<Pose> pose = htk.handmodel.GetPose();
			pt::ptree coord;
			for (int i = 0; i < pose.size(); i++) {
				coord.put("x", pose[i].position.x);
				coord.put("y", pose[i].position.y);
				coord.put("z", pose[i].position.z);
				fingers.add_child(boneNames[i], coord);
			}
		} else {
			fingers.put("thumb", std::to_string(htk.handmodel.ThumbRaised()));
			fingers.put("index", std::to_string(htk.handmodel.IndexFingerRaised()));
			fingers.put("second", std::to_string(htk.handmodel.MiddleFingerRaised()));
			fingers.put("third", std::to_string(htk.handmodel.ThirdFingerRaised()));
			fingers.put("pinky", std::to_string(htk.handmodel.PinkyFingerRaised()));
		}
		pt::ptree message;
		message.put("x", std::to_string(htk.handmodel.GetPalmLocation().x));
		message.put("y", std::to_string(htk.handmodel.GetPalmLocation().y));
		message.put("z", std::to_string(htk.handmodel.GetPalmLocation().z));
		message.add_child("fingers", fingers);
		message.put("click", false);

		struct timeval tp;
		gettimeofday(&tp, NULL);
		long int ms = tp.tv_sec * 1000 + tp.tv_usec / 1000;
		message.put("time_embedded", ms);

		std::ostringstream buf; 
		pt::write_json(buf, message, false);
		std::string json = buf.str();

		std::cout << json << std::endl;
		ws.write(boost::asio::buffer(json));
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
