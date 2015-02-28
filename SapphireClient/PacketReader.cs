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
using System.Collections.Generic;

namespace AllGoFree.SapphireEngine.Client
{
	/// <summary>
	/// Reads packets for a stream.
	/// </summary>
	public class PacketReader
	{
		/// <summary>
		/// The packet creators.
		/// </summary>
		private Dictionary<int, PacketHandler> handlers;

		/// <summary>
		/// Initializes a new instance of the <see cref="AllGoFree.SapphireEngine.Client.PacketReader"/> class.
		/// </summary>
		public PacketReader()
		{
			handlers = new Dictionary<int, PacketHandler>();
		}

		/// <summary>
		/// Handle a packet.
		/// </summary>
		/// <param name="packet">The packet.</param>
		/// <param name="stream">The stream that the packet originated from.</param>
		public void Handle(Packet packet, BlitzStream stream)
		{
			if (!handlers.ContainsKey(packet.GetId()))
			{
				// TODO: Report an error?
				return;
			}

			PacketHandler handler = handlers[packet.GetId()];

			// Handle the packet
			handler.HandlePacket(packet, stream);
		}

		/// <summary>
		/// Add a packet handler.
		/// </summary>
		/// <param name="handler">The handler.</param>
		public void Add(PacketHandler handler)
		{
			// Grab all of the IDs that the handler can read.
			int[] ids = handler.GetBinds();

			//TODO: Check if a handler is already set for any of the packet IDs.

			foreach (int next in ids)
			{
				handlers.Add(next, handler);
			}
		}

	}
}

