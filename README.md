# TCP Chat Application

A simple chat application that allows multiple clients to communicate through a TCP server. The application supports both public chat messages and private messages between specific clients.

## Features

- Multiple client connections
- Public chat messages
- Private messages to specific clients
- Letter frequency counting
- Timestamp for all messages
- System notifications for client connections/disconnections

## How to Run

1. First, start the server:
   ```bash
   cd ChatServer
   dotnet run
   ```

2. Then, start one or more clients:
   ```bash
   cd ChatClient
   dotnet run
   ```

## Usage

1. When you start a client, you'll be prompted to enter your name
2. After connecting, you can:
   - Send public messages by simply typing and pressing Enter
   - Send private messages using the format: `To: ClientName - Your message`
   - View system messages about client connections/disconnections
   - See message timestamps in the format: dd/mm/yyyy hh:mm:ss

## Message Format

- Public messages: `dd/mm/yyyy hh:mm:ss, NickName - Message content`
- Private messages: `[Private] dd/mm/yyyy hh:mm:ss, SenderName - Message content`
- System messages: `System: Message content`

## Technical Details

- Server port: 5000
- Supports multiple simultaneous client connections
- Thread-safe implementation
- Automatic client disconnection handling
- Letter frequency counting for all messages 
