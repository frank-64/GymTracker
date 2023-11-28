# GymTracker

## Introduction

This project is part of my final year major project and dissertation module for my bachelor degree in Computer Science at Newcastle University.
This is a web app that provides real-time occupancy status for a gym using webhook event requests from the KISI cloud-based access control system. The app consists of a React front-end and a .NET Core back-end API, which connects to a Cosmos DB database for data storage. The app is hosted on Azure and uses Azure functions to capture event requests from KISI when a door is opened or closed. The occupancy status is then updated in real-time on the web app.

## Features
* Real-time occupancy status updates for a gym
* Easy integration with the KISI cloud-based access control system
* Hosted on Azure for high availability and scalability
* Built with Azure functions for efficient event handling and processing
* Secure and reliable solution for gym occupancy management
* React front-end for an intuitive user interface
* .NET Core back-end API for efficient data handling
* Cosmos DB database for scalable and cost-effective data storage
* Graphs of occupancy for past week's busiest days and hours
* Equipment availability estimations for informed workout planning

# Infrastructure Architectural Design
![image](https://github.com/frank-64/GymTracker/assets/68070161/62474e55-34f0-4821-a2c2-d0e41b8cc4d9)
