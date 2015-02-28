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

namespace AllGoFree.SapphireEngine.Client
{

	/// <summary>
	/// A packet handler.
	/// </summary>
	public interface PacketHandler
	{

		/// <summary>
		/// Handle an incoming packet.
		/// </summary>
		/// <param name="packet">The packet to handle.</param>
		/// <param name="stream">The stream that produced the packet.</param>
		void HandlePacket(Packet packet, BlitzStream stream);

		/// <summary>
		/// Get the list of packet IDs that this handler should bind too.
		/// </summary>
		/// <returns>The binds.</returns>
		int[] GetBinds();

	}


}

