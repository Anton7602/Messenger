# WCF Client-Server Messenger App
# Try Connecting to 188.134.90.28:7602 - Chanсes are I'm holding open chatroom there )

This is a client-server messenger application built using WCF (Windows Communication Foundation). The application allows users to exchange messages in real-time over a direct IP:Port connection. The server-side is developed using C# WPF, while the client-side is developed using C# DevExpress. Both sides follow the MVVM (Model-View-ViewModel) pattern to separate concerns and maintain a clean architecture.

## Features

- **Real-time Messaging**: Users can send and receive messages in real-time over the network.
- **Direct IP:Port Connection**: The client connects directly to the server using the server's IP address and port number.
- **MVVM Pattern**: The application follows the MVVM pattern, ensuring separation of concerns and maintainability.
- **SQL Database Storage**: The server-side uses Entity Framework to store messages in a SQL database, providing persistence and retrieval capabilities.
- **Active Client Tracking**: The server keeps track of active clients connected to the server, allowing for targeted message delivery and status updates.
