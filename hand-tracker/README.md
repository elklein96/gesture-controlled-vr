﻿# Hand Tracking Samples

[![License is Apache 2.0](http://img.shields.io/badge/license-Apache-blue.svg?style=flat)](https://www.apache.org/licenses/LICENSE-2.0)

Platform | Build Status |
-------- | ------------ |
Visual Studio 2015 | [![Build status](https://ci.appveyor.com/api/projects/status/swutsp1bjcc56q64/branch/master?svg=true)](https://ci.appveyor.com/project/ddiakopoulos/hand-tracking-samples/branch/master)

This project provides C++ code to demonstrate hand pose estimation via depth data, namely Intel® RealSense™ depth cameras. Additionally, this project showcases the utility of convolutional neural networks as a key component of real-time hand tracking pipelines. A variety of tools and apps are provided, including an OpenVR demo showing hand tracking from an egocentrically-mounted depth camera. A YouTube video demonstrating some functionality the hand tracking code can be found [here](https://www.youtube.com/watch?v=Yqz6T9IdiNk). 

The software provided here works with the currently available Intel® RealSense™ depth cameras supported by [librealsense](https://github.com/IntelRealSense/librealsense).   
The release of this repository closely coincides with the availibility of the latest-gen [RS400](https://arxiv.org/abs/1705.05548) camera. Support for this device will added after it begins shipping.

_Hand Tracking Samples_ is experimental code and not an official Intel® product. It is subject to incompatible changes in future updates.

## Sample Applications

Each sample application comes with a separate readme further explaining its purpose and usage.

* [synthetic-hand-tracker](./synthetic-hand-tracker/) - This application demonstrates basic dynamics-based hand tracking using synthetic data produced from a 3D model rendered to an OpenGL context (and corresponding depth buffer). This application is a good entrypoint into the codebase as it does not require any specific camera hardware. 

* [realtime-hand-tracker](./realtime-hand-tracker) - This sample introduces the dynamics-based tracker in the context of real-time input from an Intel® RealSense™ depth camera. It requires a RealSense™ SR300 device. Moreover, the sample illustrates the impact of real-world sensor noise and the need for effective segmentation. 

## Building

Install CMake and Make on your system and run `make` from this directory

## Known Limitations

The project does not aim to provide a comprehensive hand pose estimation pipeline suitable for production use.

* The system must be initialized close to a known hand pose and assumes a single (right) hand only
* No sophisticated background removal or segmentation algorithms has been provided
* Localization/segmentation of the hand within the image was designed mostly for egocentric VR usage and relies on seeing where the wrist enters the image frame
* The dynamics-based tracker depends on a 3D hand model whose size and joint locations closely match the author's
* Example projects must be compiled in Release mode (-O3), otherwise the project will not run at interactive framerates

## Future Work

The approach to dynamic-based tracking was first described by Melax Et Al. in [Dynamics Based 3D Skeletal Hand Tracking (2013)](https://arxiv.org/abs/1705.07640) and later released in [binary form](https://software.intel.com/en-us/articles/the-intel-skeletal-hand-tracking-library-experimental-release) for evaluation. _Hand Tracking Samples_ implements an identical solver and builds upon dynamics-based tracking, taking inspiration from Jonathan Tompson's [Real-Time Continuous Pose Recovery of Human Hands Using Convolutional Networks (2014)](http://www.cims.nyu.edu/~tompson/others/TOG_2014_paper.pdf) as a means of further constraining pose estimates. A comprehensive survey of papers in hand pose estimation have been helpfully assembled [here](https://github.com/xinghaochen/awesome-hand-pose-estimation).

To extend this work further, there exist a number of incompletely solved steps in the pipeline:
* Localization and Segmentation - Dynamics-based tracking is not robust to input noise. The tracker should only be fed with depth points on the hand itself. Furthermore, additional work is needed to disambiguate left and right hands. Solutions in this area implicate both vision and learning based methods. 
* Model Fitting - A generic right hand model is provided in this repository. To extend this further, the pipeline should incorporate a parametric hand model fit in real-time to individual users, matching overall hand size and joint positions.
* CNN Optimization and Runtime Performance - A variety of network architectures have been explored in research for hand pose estimation. Further work in this area will analyze new network types and runtime performance -- both forward execution speed and trained network sizes -- to find an ideal balance between online dynamics-based tracking and CNN-based correction.

## License

Copyright 2017 Intel Corporation

Licensed under the Apache License, Version 2.0 (the "License");
you may not use this project except in compliance with the License.
You may obtain a copy of the License at

    http://www.apache.org/licenses/LICENSE-2.0

Unless required by applicable law or agreed to in writing, software
distributed under the License is distributed on an "AS IS" BASIS,
WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
See the License for the specific language governing permissions and
limitations under the License.
