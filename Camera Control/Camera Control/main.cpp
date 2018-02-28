// License: Apache 2.0. See LICENSE file in root directory.
// Copyright(c) 2015 Intel Corporation. All Rights Reserved.

/////////////////////////////////////////////////////
// librealsense tutorial #1 - Accessing depth data //
/////////////////////////////////////////////////////

// First include the librealsense C++ header file
#include <csignal>
#include <librealsense/rs.hpp>
#include <cstdio>
#include <boost/beast/core.hpp>
#include <boost/beast/websocket.hpp>
#include <boost/asio/connect.hpp>
#include <boost/asio/ip/tcp.hpp>

using tcp = boost::asio::ip::tcp;
namespace websocket = boost::beast::websocket;

int f_shutdown;

void shutdown_handler(int x)
{
    f_shutdown = 1;
}

int main() try
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

    // Create a context object. This object owns the handles to all connected realsense devices.
    rs::context ctx;
    printf("There are %d connected RealSense devices.\n", ctx.get_device_count());
    if(ctx.get_device_count() == 0) return EXIT_FAILURE;

    rs::device * dev = ctx.get_device(0);
    printf("\nUsing device 0, an %s\n", dev->get_name());
    printf("    Serial number: %s\n", dev->get_serial());
    printf("    Firmware version: %s\n", dev->get_firmware_version());

    // Configure depth to run at VGA resolution at 30 frames per second
    dev->enable_stream(rs::stream::depth, 640, 480, rs::format::z16, 30);

    dev->start();

    // Determine depth value corresponding to one meter
    const uint16_t one_meter = static_cast<uint16_t>(1.0f / dev->get_depth_scale());

    while(true)
    {
        if (f_shutdown)
        {
            printf("SHUTTING DOWN GRACEFULLY...\n");
            ws.close(websocket::close_code::normal);
            dev->stop();
        }
        
        // This call waits until a new coherent set of frames is available on a device
        // Calls to get_frame_data(...) and get_frame_timestamp(...) on a device will return stable values until wait_for_frames(...) is called
        dev->wait_for_frames();

        // Retrieve depth data, which was previously configured as a 640 x 480 image of 16-bit depth values
        const uint16_t * depth_frame = reinterpret_cast<const uint16_t *>(dev->get_frame_data(rs::stream::depth));

        // Print a simple text-based representation of the image, by breaking it into 10x20 pixel regions and and approximating the coverage of pixels within one meter
        char buffer[(640/10+1)*(480/20)+1];
        char * out = buffer;
        int coverage[64] = {};
        for(int y=0; y<480; ++y)
        {
            for(int x=0; x<640; ++x)
            {
                int depth = *depth_frame++;
                if(depth > 0 && depth < one_meter) ++coverage[x/10];
            }

            if(y%20 == 19)
            {
                for(int & c : coverage)
                {
                    *out++ = " .:nhBXWW"[c/25];
                    c = 0;
                }
                *out++ = '\n';
            }
        }
        
        int chars = 0;
        for (int i = 0; i < 1561; ++i)
        {
            if (buffer[i] != ' ')
            {
                chars++;
            }
        }

        float objectThreshold = (float)chars / (float)1561;
        
        if (objectThreshold > 0.2) {
//            printf("DEBUG:: send message %f\n", objectThreshold);
            ws.write(boost::asio::buffer(std::string("1")));
        } else {
            ws.write(boost::asio::buffer(std::string("0")));
        }

        *out++ = 0;
        printf("\n%s", buffer);
    }

    return EXIT_SUCCESS;
}
catch(const rs::error & e)
{
    // Method calls against librealsense objects may throw exceptions of type rs::error
    printf("rs::error was thrown when calling %s(%s):\n", e.get_failed_function().c_str(), e.get_failed_args().c_str());
    printf("    %s\n", e.what());
    return EXIT_FAILURE;
}
