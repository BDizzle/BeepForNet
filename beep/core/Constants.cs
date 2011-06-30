/*
* Constants.java  $Revision: 1.1 $ $Date: 2004/09/21 08:46:56 $
*
* Copyright (c) 2001 Invisible Worlds, Inc.  All rights reserved.
*
* The contents of this file are subject to the Blocks Public License (the
* "License"); You may not use this file except in compliance with the License.
*
* You may obtain a copy of the License at http://www.beepcore.org/
*
* Software distributed under the License is distributed on an "AS IS" basis,
* WITHOUT WARRANTY OF ANY KIND, either express or implied.  See the License
* for the specific language governing rights and limitations under the
* License.
*
*/
using System;
namespace beepcore.beep.core
{
	/// <summary>A placeholder class for all our constants, both for ease
	/// of use, for potential management later, and to minimize string creation.</summary>
	/// <author>Eric Dixon</author>
	/// <author>Huston Franklin</author>
	/// <author>Jay Kint</author>
	/// <author>Scott Pead</author>
	/// <version>$Revision: 1.1 $, $Date: 2004/09/21 08:46:56 $</version>
	internal class StringConstants
	{
		public const string ENCODING_NONE = "none";
		public const string ENCODING_BASE64 = "base64";
		public const string ENCODING_DEFAULT = ENCODING_NONE;
		public const string LOCALIZE_DEFAULT = "i-default";
		public const string LOCALIZE_US = "en-us";
	}
}