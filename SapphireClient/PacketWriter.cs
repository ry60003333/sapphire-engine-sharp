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
	public class PacketWriter
	{
		/// <summary>
		/// The packet creators.
		/// </summary>
		private Dictionary<int, PacketCreator> creators;

		/// <summary>
		/// Initializes a new instance of the <see cref="AllGoFree.SapphireEngine.Client.PacketWriter"/> class.
		/// </summary>
		public PacketWriter()
		{
			creators = new Dictionary<int, PacketCreator>();
		}

		/// <summary>
		/// Write a packet to a stream.
		/// </summary>
		/// <param name="stream">The stream.</param>
		/// <param name="id">The packet ID.</param>
		/// <param name="parameters">The parameters.</param>
		public void Write(BlitzStream stream, int id)
		{
			Write(stream, id, new object[] {});
		}

		/// <summary>
		/// Write a packet to a stream.
		/// </summary>
		/// <param name="stream">The stream.</param>
		/// <param name="id">The packet ID.</param>
		/// <param name="parameters">The parameters.</param>
		public void Write(BlitzStream stream, int id, object[] parameters)
		{
			if (!creators.ContainsKey(id))
			{
				throw new Exception("Unknown packet ID: " + id);
			}

			PacketCreator creator = creators[id];

			Packet packet = new Packet(id);
			creator.Write(packet, parameters);
			packet.FinalizePacket();
			stream.WritePacket(packet);
			stream.WriteRequired();
		}

		/// <summary>
		/// Add a packet creator.
		/// </summary>
		/// <param name="creator">The packet creator.</param>
		public void Add(PacketCreator creator)
		{
			// Grab all of the IDs that the creator can make.
			int[] ids = creator.GetBinds();

			// TODO: Check if a creator is already set for any of the packet IDs.

			foreach (int next in ids)
			{
				creators.Add(next, creator);
			}
		}
	}
}

