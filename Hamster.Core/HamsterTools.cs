﻿using MimeMapping;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hamster.Core
{
	public class HamsterTools
	{
		public static string GetContentType(string filename)
		{
			return MimeUtility.GetMimeMapping(filename);
		}
		public static string RouteToPath(string root, string url)
		{
			string[] paths = url.Substring(1).Split('/');
			string path = root;
			foreach (string s in paths)
			{
				path = Path.Combine(path, s);
			}
			return path;
		}
	}
}
