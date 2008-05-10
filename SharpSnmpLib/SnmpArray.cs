/*
 * Created by SharpDevelop.
 * User: lextm
 * Date: 2008/4/30
 * Time: 20:53
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;

namespace Lextm.SharpSnmpLib
{
	/// <summary>
	/// Array type.
	/// </summary>
	/// <remarks>Represents SMIv1 SEQUENCE.</remarks>
	public class SnmpArray: ISnmpData
	{
		byte[] _bytes;
		byte[] _raw;		
		IList<ISnmpData> _list = new List<ISnmpData>();
		/// <summary>
		/// Creates an <see cref="SnmpArray"/> instance with varied <see cref="ISnmpData"/> instances.
		/// </summary>
		/// <param name="items"></param>
		public SnmpArray(params ISnmpData[] items)
		{
			foreach (ISnmpData item in items)
			{
				_list.Add(item);
			}
			_raw = ByteTool.ParseItems(items);
		}
		/// <summary>
		/// Creates an <see cref="SnmpArray"/> instance with varied <see cref="ISnmpData"/> instances.
		/// </summary>
		/// <param name="items"></param>
		public SnmpArray(IEnumerable items)
		{
			if (!(items is IEnumerable<ISnmpData>)) 
			{
				throw new ArgumentException("objects must be IEnumerable<ISnmpData>");
			}
			foreach (ISnmpData item in items)
			{
				_list.Add(item);
			}
			_raw = ByteTool.ParseItems(items);
		}
		/// <summary>
		/// Creates an <see cref="SnmpArray"/> instance from raw bytes.
		/// </summary>
		/// <param name="raw">Raw bytes</param>
		public SnmpArray(byte[] raw)
		{
			_raw = raw;
			if (raw.Length != 0) {
				MemoryStream m = new MemoryStream(raw);
				while (m.Position < raw.Length)
				{
					_list.Add(SnmpDataFactory.CreateSnmpData(m));
				}
			}
		}
		/// <summary>
		/// <see cref="ISnmpData"/> instances containing in this <see cref="SnmpArray"/>
		/// </summary>
		public IList<ISnmpData> Items
		{
			get
			{
				return _list;
			}
		}
		/// <summary>
		/// Type code.
		/// </summary>
		public SnmpType TypeCode {
			get {
				return SnmpType.Array;
			}
		}
		/// <summary>
		/// To byte format.
		/// </summary>
		/// <returns></returns>
		public byte[] ToBytes()
		{
			if (null == _bytes) {
				MemoryStream result = new MemoryStream();
				result.WriteByte((byte)TypeCode);
				ByteTool.WriteMultiByteLength(result, _raw.Length); //it seems that trap does not use this function
				result.Write(_raw,0,_raw.Length);
				_bytes = result.ToArray();
			}
			return _bytes;
		}
	}
}
