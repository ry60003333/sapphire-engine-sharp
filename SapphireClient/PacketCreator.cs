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
	/// Creates a packet.
	/// </summary>
	public interface PacketCreator
	{

		/// <summary>
		/// Write data to a newly created packet.
		/// </summary>
		/// <param name="packet">The packet to write too.</param>
		/// <param name="parameters">The parameters for writing the packet.</param>
	 	void Write(Packet packet, object[] parameters);

		/// <summary>
		/// Get the IDs of the packets that the class creates.
		/// </summary>
		/// <returns>The IDs of the packets.</returns>
		int[] GetBinds();
	}
}

