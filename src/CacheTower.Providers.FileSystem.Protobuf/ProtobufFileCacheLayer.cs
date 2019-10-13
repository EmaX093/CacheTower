﻿using System;
using System.IO;
using System.Threading.Tasks;
using ProtoBuf;
using ProtoBuf.Meta;

namespace CacheTower.Providers.FileSystem.Protobuf
{
	public class ProtobufFileCacheLayer : FileCacheLayerBase<ProtobufManifestEntry>
	{
		public ProtobufFileCacheLayer(string directoryPath) : base(directoryPath, ".bin") { }

		public static void ConfigureProtobuf()
		{
			RuntimeTypeModel.Default.Add(typeof(IManifestEntry))
				.AddSubType(1, typeof(ProtobufManifestEntry));
		}

		protected override async Task<T> Deserialize<T>(Stream stream)
		{
			using (var memStream = new MemoryStream((int)stream.Length))
			{
				await stream.CopyToAsync(memStream);
				memStream.Seek(0, SeekOrigin.Begin);
				return Serializer.Deserialize<T>(memStream);
			}
		}

		protected override async Task Serialize<T>(Stream stream, T value)
		{
			using (var memStream = new MemoryStream())
			{
				Serializer.Serialize(memStream, value);
				memStream.Seek(0, SeekOrigin.Begin);
				await memStream.CopyToAsync(stream);
			}
		}
	}
}