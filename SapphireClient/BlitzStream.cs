/*
 * Copyright (c) Ryan Rule-Hoffman, All rights reserved.
 * The Sapphire C# Client
 * 
 * This library is free software; you can redistribute it and/or
 * modify it under the terms of the GNU Lesser General Public
 * License as published by the Free Software Foundation; either
 * version 3.0 of the License, or (at your option) any later version.
 * 
 * This library is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
 * Lesser General Public License for more details.
 * 
 * You should have received a copy of the GNU Lesser General Public
 * License along with this library.
 */

using System;
using System.Net;
using System.Net.Sockets;
using System.Collections;

namespace AllGoFree.SapphireEngine.Client
{
	/// <summary>
	/// A stream connected to a client.
	/// </summary>
	public class BlitzStream
	{
		/// <summary>
		/// Gets the packet reader.
		/// </summary>
		/// <value>The packet reader.</value>
		public PacketReader PacketReader
		{ 
			get
			{
				return packetReader;
			} 
		}

		/// <summary>
		/// Gets the packet writer.
		/// </summary>
		/// <value>The packet writer.</value>
		public PacketWriter PacketWriter
		{
			get
			{
				return packetWriter;
			}
		}

		/// <summary>
		/// Gets or sets the attachment.
		/// </summary>
		/// <value>The attachment.</value>
		public object Attachment
		{
			get;
			set;
		}
		
		/// <summary>
		/// The size of the read and write buffers.
		/// </summary>
		private static readonly int BUFFER_SIZE = 1024;

		/// <summary>
		/// The underlying socket.
		/// </summary>
		private readonly Socket socket;

		/// <summary>
		/// The packet reader.
		/// </summary>
		private readonly PacketReader packetReader;

		/// <summary>
		/// The packet writer.
		/// </summary>
		private readonly PacketWriter packetWriter;

		/// <summary>
		/// The packet that serves as the read buffer.
		/// </summary>
		private readonly Packet readBuffer;

		/// <summary>
		/// The packet that serves as the write buffer.
		/// </summary>
		private readonly Packet writeBuffer;

		/// <summary>
		/// Should packets be queued instead of directly read.
		/// </summary>
		private readonly bool queuePackets;

		/// <summary>
		/// The queue of packets to read.
		/// </summary>
		private readonly Queue readQueue;

		/// <summary>
		/// The queue of packets to write.
		/// </summary>
		private readonly Queue writeQueue;

		/// <summary>
		/// Is a write currently in progress.
		/// </summary>
		private bool writeInProgress = false;

		/// <summary>
		/// Initializes a new instance of the <see cref="AllGoFree.SapphireEngine.Client.BlitzStream"/> class.
		/// </summary>
		/// <param name="socket">The underlying Socket..</param>
		public BlitzStream(Socket socket)
		{
			this.socket = socket;
			packetReader = new PacketReader();
			packetWriter = new PacketWriter();
			readBuffer = new Packet(new byte[BUFFER_SIZE]);
			writeBuffer = new Packet(new byte[BUFFER_SIZE]);
			readQueue = Queue.Synchronized(new Queue());
			writeQueue = Queue.Synchronized(new Queue());
			queuePackets = true;


		}

		/// <summary>
		/// Start reading data from the stream.
		/// </summary>
		public void Start()
		{
			// Start reading in data from the socket
			socket.BeginReceive(readBuffer.GetBuffer(), readBuffer.GetOffset(), readBuffer.GetBuffer().Length, SocketFlags.None, new AsyncCallback(DataReceived), socket);
		}

		/// <summary>
		/// Handle packets in the read queue.
		/// </summary>
		public void HandlePackets()
		{
			while (readQueue.Count > 0)
			{
				Packet next = (Packet)readQueue.Dequeue();
				packetReader.Handle(next, this);

				//Debug.Log("Handled packet ID " + next.GetId() + " of size " + next.GetBuffer().Length + " on game thread.");
			}
		}

		/// <summary>
		/// Called when data is received from the server.
		/// </summary>
		/// <param name="result">The results.</param>
		void DataReceived(IAsyncResult result)
		{
			Socket remote = (Socket)result.AsyncState;

			// Check how many bytes were actually read
			int count = remote.EndReceive(result);

			// Debugging
			//Debug.Log("Received " + count + " bytes from server.");

			// Update the buffer offset to account for the amount of data read in
			readBuffer.Seek(readBuffer.GetOffset() + count);

			// TODO: If the while below is an if statement (as it was before)
			// and more then one packet arrives, the Stream appears to lock up.
			// The while loop is a workaround for now, but this needs to be
			// fixed.

			// A packet requires a minimum of 3 bytes for the header
			while (readBuffer.GetOffset() >= 3)
			{

				// Save the current write offset of the buffer
				int writeOffset = readBuffer.GetOffset();

				// Rewind to the beginning
				readBuffer.Rewind();

				// Read the next packet ID and size
				int id = readBuffer.ReadUnsignedByte();
				int size = readBuffer.ReadUnsignedWord();

				// Make sure the buffer can hold the packet
				if (readBuffer.GetBuffer().Length < size)
				{
					//TODO: Auto expand the buffer
					throw new Exception("Packet ID " + id + " too large: " + size + " bytes.");
				}

				// Calculate the size of the currently stored payload in the buffer
				// we do this by subtracting 3 bytes (1 for the packet ID, 2 for the size)
				int currentPayloadSize = writeOffset - 3;

				// If we have all of the data, read in the packet!
				if (currentPayloadSize >= size)
				{
					// Create a new byte array to hold the payload
					byte[] payload = new byte[size];

					// Read in the payload
					Array.Copy(readBuffer.GetBuffer(), readBuffer.GetOffset(), payload, 0, size);

					// Seek to the end of the data
					readBuffer.Seek(readBuffer.GetOffset() + size);

					// Purge the data
					readBuffer.Purge();

					// Correct the write offset, removing the packet header and the payload size
					writeOffset -= (size + 3);

					// Create the actual packet object
					Packet packet = new Packet(id, payload);

					// Yay for debugging!
					//Debug.Log("Read packet ID " + packet.GetId() + " of size " + payload.Length);

					if (queuePackets)
					{
						// Place packets in a Queue for the game thread to handle
						readQueue.Enqueue(packet);
					}
					else
					{
						// Directly handle the packet
						packetReader.Handle(packet, this);
					}
				}
				else
				{
					// Otherwise, break out of the loop

					// Debugging
					// Debug.Log("Waiting for packet " + id + " of size " + size + ".");

					break;
				}

				// Go back to the write offset
				readBuffer.Seek(writeOffset);

			}

			// Receive more data!
			socket.BeginReceive(readBuffer.GetBuffer(), readBuffer.GetOffset(), readBuffer.GetBuffer().Length, SocketFlags.None, new AsyncCallback(DataReceived), socket);
		}

		/// <summary>
		/// Write a packet to the stream.
		/// </summary>
		/// <param name="packet">The packet.</param>
		internal void WritePacket(Packet packet)
		{
			writeQueue.Enqueue(packet);
			WriteRequired();
		}

		/// <summary>
		/// Called when a write is required.
		/// </summary>
	 	internal void WriteRequired()
		{
			// Make sure a write isn't already in progress, 
			// and that we actually have packets to write
			if (writeInProgress)
			{
				return;
			}

			// Flag that a write is in progress
			writeInProgress = true;

			// Check if any packets are waiting to be written
			// TODO: Change this to a while loop, so all queued packets are combined and sent
			if (writeQueue.Count > 0)
			{
				// Grab the packet to write
				Packet packet = (Packet)writeQueue.Dequeue();

				// Make sure the packet is finalized
				packet.FinalizePacket();

				// Make sure the packet fits
				if (writeBuffer.Remaining() < packet.GetBuffer().Length + 3)
				{
					// TODO: Autoexpand write buffer
					throw new Exception("Packet is too fat, ID: " + packet.GetId());
				}

				// Write the packet to the write buffer
				writeBuffer.WriteByte(packet.GetId());
				writeBuffer.WriteWord(packet.GetBuffer().Length);

				//Debug.Log("Writing packet ID " + packet.GetId() + " of size " + packet.GetBuffer().Length + " to server.");

				// Write the payload
				Array.Copy(packet.GetBuffer(), 0, writeBuffer.GetBuffer(), writeBuffer.GetOffset(), packet.GetBuffer().Length);

				// Fix the offset in the write buffer
				writeBuffer.Seek(writeBuffer.GetOffset() + packet.GetBuffer().Length);
			}

			// Make sure we actually have data to write
			if (writeBuffer.GetOffset() == 0)
			{
				writeInProgress = false;
				return;
			}

			// Attempt to write the write buffer to the network
			socket.BeginSend(writeBuffer.GetBuffer(), 0, writeBuffer.GetOffset(), SocketFlags.None, new AsyncCallback(DataSent), socket);

		}

		/// <summary>
		/// Called when data is sent.
		/// </summary>
		/// <param name="result">The results.</param>
		void DataSent(IAsyncResult result)
		{
			Socket remote = (Socket)result.AsyncState;

			// Grab the amount of bytes that were actually written
			int count = remote.EndSend(result);

			//Debug.Log("Sent " + count + " bytes to server.");

			// Grab the current write offset
			int writeOffset = writeBuffer.GetOffset();

			// Seek to the amount of bytes written
			writeBuffer.Seek(count);

			// Purge the written data
			writeBuffer.Purge();

			// Seek back to the corrected write position
			writeBuffer.Seek(writeOffset - count);

			// Write complete!
			writeInProgress = false;

			// Check if another write is required
			WriteRequired();
		}


	}
}

