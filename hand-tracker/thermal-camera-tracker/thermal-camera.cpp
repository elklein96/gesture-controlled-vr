#include <Python.h>
#include <vector>
#include <iostream>
#include <stdexcept>

#include <boost/beast/core.hpp>
#include <boost/beast/websocket.hpp>
#include <boost/asio/connect.hpp>
#include <boost/asio/ip/tcp.hpp>
#include <boost/property_tree/ptree.hpp>
#include <boost/property_tree/json_parser.hpp>

#include "../include/handtrack.h"
#include "../include/dcam.h"

using tcp = boost::asio::ip::tcp;
namespace websocket = boost::beast::websocket;
namespace pt = boost::property_tree;

auto const HOST = "18.220.146.229";
auto const PORT = "3001";

int f_shutdown;

void shutdown_handler(int x) {
    f_shutdown = 1;
}

websocket::stream<tcp::socket> setup_socket() {
    boost::asio::io_service ios;
    websocket::stream<tcp::socket> ws{ios};

    tcp::resolver resolver(ios);
    tcp::resolver::query query(HOST, PORT);
    auto const lookup = resolver.resolve(query);

    boost::asio::connect(ws.next_layer(), lookup);
    ws.handshake(HOST, "/");

    return ws;
}

std::vector<uint16_t> listTupleToVector_Float(PyObject *incoming) {
    std::vector<uint16_t> data;
    if (PyTuple_Check(incoming)) {
        for (Py_ssize_t i = 0; i < PyTuple_Size(incoming); i++) {
            PyObject *value = PyTuple_GetItem(incoming, i);
            data.push_back(PyFloat_AsDouble(value));
        }
    } else {
        if (PyList_Check(incoming)) {
            for (Py_ssize_t i = 0; i < PyList_Size(incoming); i++) {
                PyObject *value = PyList_GetItem(incoming, i);
                data.push_back(PyFloat_AsDouble(value));
            }
        } else {
            throw std::logic_error("Passed PyObject pointer was not a list or tuple!");
        }
    }
    return data;
}

int main(int argc, char *argv[]) {
    PyObject *pName, *Adafruit_AMG88xx, *sensorAttr, *sensor, *pixels, *pValue;

    RSCam dcam;
    HandTracker htk;

    bool run = true;
    bool sendMeshDataFlag = false;

    std::string const boneNames[] = {
        "wrist", "palm",
        "thumb_1", "thumb_2", "thumb_3",
        "index_1", "index_2", "index_3",
        "middle_1", "middle_2", "middle_3",
        "third_1", "third_2", "third_3",
        "pinky_1", "pinky_2", "pinky_3", 
    };

    // First, create a signal handler for graceful shutdowns
    signal(SIGINT, shutdown_handler);
    f_shutdown = 0;

    // Set up the HTTP Socket Client
    websocket::stream<tcp::socket> ws = setup_socket();
    ws.write(boost::asio::buffer(std::string("CAMERA:: CONNECTED")));

    htk.load_config( "../config.json");

    Py_SetProgramName(argv[0]);
    Py_Initialize();

    while (run) {
        if (f_shutdown) {
            std::cout << "SHUTTING DOWN GRACEFULLY..." << std::endl;
            ws.close(websocket::close_code::normal);
            run = false;
            break;
        }

        pName = PyString_FromString("Adafruit_AMG88xx");

        Adafruit_AMG88xx = PyImport_Import(pName);

        sensorAttr = PyObject_GetAttrString(Adafruit_AMG88xx, "Adafruit_AMG88xx");
        sensor = PyObject_CallObject(sensorAttr, NULL);

        pixels = PyObject_GetAttrString(sensor, "readPixels");
        pValue = PyObject_CallObject(pixels, NULL);

        std::vector<uint16_t> ddata = listTupleToVector_Float(pValue);
        Image<unsigned short> dimage = dcam.FilterDS4(ddata.data());

        // for (int i = 0; i < ddata.size(); ++i)
        //     std::cout << ddata[i] << ' ';
        // std::cout << std::endl << std::endl;

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

        std::ostringstream buf;
        pt::write_json(buf, message, false);
        std::string json = buf.str();

        std::cout << json << std::endl;
        ws.write(boost::asio::buffer(json));
    }

    std::cout << "ABOUT TO EXIT" << std::endl;

    Py_DECREF(pValue);
    Py_DECREF(pixels);
    Py_DECREF(sensor);
    Py_DECREF(sensorAttr);
    Py_DECREF(Adafruit_AMG88xx);
    Py_DECREF(pName);

    Py_Finalize();
    return 0;
}

