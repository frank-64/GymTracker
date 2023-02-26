# GymTracker

## Introduction

This project is part of my final year major project and dissertation module for my bachelor degree in Computer Science at Newcastle University.
This is a web app that provides real-time occupancy status for a gym using webhook event requests from the KISI cloud-based access control system. The app consists of a React front-end and a .NET Core back-end API, which connects to a Cosmos DB database for data storage. The app is hosted on Azure and uses Azure functions to capture event requests from KISI when a door is opened or closed. The occupancy status is then updated in real-time on the web app. Additionally, the app includes graphs of occupancy for the past week's busiest days and hours, as well as estimations towards equipment availability.

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
