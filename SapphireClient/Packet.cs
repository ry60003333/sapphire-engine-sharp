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

using System.Collections;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System;

namespace AllGoFree.SapphireEngine.Client
{
	/// <summary>
	/// A packet.
	/// </summary>
	public class Packet
	{
		/// <summary>
		/// The default buffer size.
		/// </summary>
		private static readonly int DEFAULT_BUFFER_SIZE = 256;

		/// <summary>
		/// The ID of the packet.
		/// </summary>
		private int id;
		
		/// <summary>
		/// The bytes of the packet.
		/// </summary>
		private byte[] buf;
		
		/// <summary>
		/// The current buffer offset.
		/// </summary>
		private int offset;

		/// <summary>
		/// Has the packet been finalized.
		/// </summary>
		private bool finalized;
		
		/// <summary>
		/// Creates a new Packet.
		/// </summary>
		/// <param name="id">The ID of the packet.</param>
		/// <param name="buf">The byte buffer of the packet.</param>
		public Packet(int id, byte[] buf)
		{
			this.id = id;
			this.buf = buf;
		}
		
		/// <summary>
		/// Creates a new Packet.
		/// </summary>
		/// <param name="id">The ID of the packet.</param>
		public Packet(int id)
		{
			this.id = id;
			this.buf = new byte[DEFAULT_BUFFER_SIZE];
		}
		
		/// <summary>
		/// Creates a new Packet.
		/// </summary>
		/// <param name="buf">The byte buffer of the packet.</param>
		public Packet(byte[] buf)
		{
			this.id = -1;
			this.buf = buf;
		}
		
		/// <summary>
		/// Write the packet to the specified client.
		/// </summary>
		/// <param name="client">The client.</param>
		public void WriteTo(TcpClient client)
		{
			NetworkStream stream = client.GetStream();
			// Write the packet ID
			stream.WriteByte((byte)id);
			
			// Write the size of the packet as a word
			stream.WriteByte((byte)(offset >> 8));
			stream.WriteByte((byte)(offset));
			
			// Write the data
			stream.Write(buf, 0, offset);
			
			// Flush!
			stream.Flush();
		}
		
		/// <summary>
		/// Get the ID of the packet.
		/// </summary>
		/// <returns>The ID.</returns>
		public int GetId()
		{
			return id;
		}
		
		/// <summary>
		/// Get the underlying buffer.
		/// </summary>
		/// <returns>The buffer.</returns>
		public byte[] GetBuffer()
		{
			return buf;
		}
		
		/// <summary>
		/// Get the current offset.
		/// </summary>
		/// <returns>The current offset.</returns>
		public int GetOffset()
		{
			return offset;
		}
		
		/// <summary>
		/// Read a signed byte from the stream.
		/// </summary>
		/// <returns>The value.</returns>
		public byte ReadSignedByte()
		{
			return buf[offset++];
		}
		
		/// <summary>
		/// Read an unsigned byte from the stream.
		/// </summary>
		/// <returns>The value.</returns>
		public int ReadUnsignedByte()
		{
			return buf[offset++] & 0xff;
		}
		
		/// <summary>
		/// Write a byte to the stream.
		/// </summary>
		/// <param name="i">The value.</param>
		/// <returns>The packet.</returns>
		public Packet WriteByte(int i)
		{
			if (finalized)
			{
				throw new Exception("Cannot write to a finalized packet.");
			}
			if (offset == buf.Length - 1)
			{
				// Automatically expand the buffer
				byte[] newBuffer = new byte[buf.Length + DEFAULT_BUFFER_SIZE];
				Array.Copy(buf, newBuffer, offset);
				buf = newBuffer;
			}
			buf[offset++] = (byte)i;
			return this;
		}
		
		/// <summary>
		/// Read an unsigned word from the stream.
		/// </summary>
		/// <returns>The value.</returns>
		public int ReadUnsignedWord()
		{
			offset += 2;
			return ((buf[offset - 2] & 0xff) << 8) + (buf[offset - 1] & 0xff);
		}
		
		/// <summary>
		/// Write a word to the stream.
		/// </summary>
		/// <param name="i">The value.</param>
		/// <returns>The packet.</returns>
		public Packet WriteWord(int i)
		{
			WriteByte((byte)(i >> 8));
			WriteByte((byte)(i));
			return this;
		}
		
		/// <summary>
		/// Read a double word.
		/// </summary>
		/// <returns>The value.</returns>
		public int ReadDWord()
		{
			offset += 4;
			return ((buf[offset - 4] & 0xff) << 24) + ((buf[offset - 3] & 0xff) << 16) + ((buf[offset - 2] & 0xff) << 8) + (buf[offset - 1] & 0xff);
		}
		
		/// <summary>
		/// Write a double word.
		/// </summary>
		/// <param name="i">The value.</param>
		/// <returns>The packet.</returns>
		public Packet WriteDWord(int i)
		{
			WriteByte((byte)(i >> 24));
			WriteByte((byte)(i >> 16));
			WriteByte((byte)(i >> 8));
			WriteByte((byte)(i));
			return this;
		}
		
		/// <summary>
		/// Read a byte array.
		/// </summary>
		/// <returns>The array.</returns>
		public byte[] ReadByteArray()
		{
			int size = ReadDWord();
			byte[] array = new byte[size];
			for (int i = 0; i < size; i++)
			{
				array[i] = ReadSignedByte();
			}
			return array;
		}
		
		/// <summary>
		/// Write a byte array.
		/// </summary>
		/// <param name="array">The array.</param>
		/// <returns>The packet.</returns>
		public Packet WriteByteArray(byte[] array)
		{
			WriteDWord(array.Length);
			foreach (byte i in array)
			{
				WriteByte(i);
			}
			return this;
		}
		
		/// <summary>
		/// Read a string.
		/// </summary>
		/// <returns>The string.</returns>
		public string ReadStringUTF32()
		{
			byte[] bytes = ReadByteArray();
			return Encoding.UTF32.GetString(bytes);
		}
		
		/// <summary>
		/// Write a string.
		/// </summary>
		/// <param name="s">The string.</param>
		/// <returns>The packet.</returns>
		public Packet WriteStringUTF32(string s)
		{
			byte[] bytes = Encoding.UTF32.GetBytes(s);
			WriteByteArray(bytes);
			return this;
		}
		
		/// <summary>
		/// Read a boolean.
		/// </summary>
		/// <returns>The value.</returns>
		public bool ReadBoolean()
		{
			return ReadUnsignedByte() == 1;
		}
		
		/// <summary>
		/// Write a boolean.
		/// </summary>
		/// <param name="b">The value.</param>
		/// <returns>The packet.</returns>
		public Packet WriteBoolean(bool b)
		{
			WriteByte(b ? 1 : 0);
			return this;
		}

		/// <summary>
		/// Read a long.
		/// </summary>
		/// <returns>The value.</returns>
		public long ReadLong()
		{
			long l = (long)ReadDWord() & 0xffffffffL;
			long l1 = (long)ReadDWord() & 0xffffffffL;
			return (l << 32) + l1;
		}

		/// <summary>
		/// Write a long.
		/// </summary>
		/// <param name="l">The long.</param>
		/// <returns>The packet.</returns>
		public Packet WriteLong(long l)
		{
			WriteByte((byte)(int)(l >> 56));
			WriteByte((byte)(int)(l >> 48));
			WriteByte((byte)(int)(l >> 40));
			WriteByte((byte)(int)(l >> 32));
			WriteByte((byte)(int)(l >> 24));
			WriteByte((byte)(int)(l >> 16));
			WriteByte((byte)(int)(l >> 8));
			WriteByte((byte)(int)l);
			return this;
		}

		/// <summary>
		/// Finalizes the packet.
		/// </summary>
		public void FinalizePacket()
		{
			// If the packet is already finalized, don't do anything
			if (finalized)
			{
				return;
			}
			byte[] newBuffer = new byte[offset];
			Array.Copy(buf, newBuffer, offset);
			buf = newBuffer;
			finalized = true;
		}

		/// <summary>
		/// Seek to the specified position.
		/// </summary>
		/// <param name="position">The position.</param>
		public void Seek(int position)
		{
			offset = position;
		}

		/// <summary>
		/// Rewind to the beginning of the buffer.
		/// </summary>
		public void Rewind()
		{
			Seek(0);
		}

		/// <summary>
		/// Purge all data before the offset and reset the offset to 0.
		/// </summary>
		public void Purge()
		{
			byte[] newBuffer = new byte[buf.Length];
			Array.Copy(buf, offset, newBuffer, 0, buf.Length - offset);
			buf = newBuffer;
			offset = 0;
		}

		/// <summary>
		/// Get the amount of bytes remaining after the current offset.
		/// </summary>
		public int Remaining()
		{
			return buf.Length - offset;
		}

	}
}