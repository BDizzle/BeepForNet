/*
* SessionTuningProperties.java  $Revision: 1.1 $ $Date: 2004/09/21 08:46:56 $
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
	
	
	/// <summary>Class SessionTuningProperties</summary>
	/// <author>Eric Dixon</author>
	/// <author>Huston Franklin</author>
	/// <author>Jay Kint</author>
	/// <author>Scott Pead</author>
	/// <version>$Rev$, $Date: 2004/09/21 08:46:56 $</version>
	public class SessionTuningProperties
	{
		virtual internal bool Empty
		{
			get
			{
				return (properties.Count == 0);
			}
			
		}
		
		// Standard settings
		public const string ENCRYPTION = "ENCRYPTION";
		//UPGRADE_NOTE: Final was removed from the declaration of 'STANDARD_PROPERTIES '. 'ms-help://MS.VSCC.2003/commoner/redir/redirect.htm?keyword="jlca1003"'
		public static readonly string[] STANDARD_PROPERTIES = new string[]{ENCRYPTION};
		
		// Pretty Print Crap
		public const string LEFT = "[";
		public const string RIGHT = "]";
		public const string MIDDLE = "=>";
		public const string NEWLINE = "\n";
		public const string NO_PROPERTIES = "SessionTuningProperties: No properties";
		
		//UPGRADE_NOTE: Final was removed from the declaration of 'emptyTuningProperties '. 'ms-help://MS.VSCC.2003/commoner/redir/redirect.htm?keyword="jlca1003"'
		internal static readonly SessionTuningProperties emptyTuningProperties = new SessionTuningProperties();
		
		// Data
		private System.Collections.Hashtable properties;
		
		/// <summary>Constructor SessionTuningProperties</summary>
		public SessionTuningProperties()
		{
			properties = new System.Collections.Hashtable();
		}
		
		/// <summary>Constructor SessionCredential</summary>
		/// <param name="props"></param>
		public SessionTuningProperties(System.Collections.Hashtable props)
		{
			properties = props;
		}
		
		/// <summary>generic way to get the value of a property.  It is recommended
		/// to use the standard methods for standard properties and that
		/// these methods only be used for non standard properties.</summary>
		public virtual object getProperty(string property)
		{
			return properties[property];
		}
		
		/// <summary>generic way to set the value of a property.  It is recommended
		/// to use the standard methods for standard properties and that
		/// these methods only be used for non standard properties.</summary>
		public virtual void  setProperty(string property, string value_Renamed)
		{
			properties[property] = value_Renamed;
		}
		
		/// <summary>gets the status of encryption</summary>
		public virtual bool getEncrypted()
		{
			return ((string) properties[ENCRYPTION]).Equals("true");
		}
		
		/// <summary>sets the status of encryption</summary>
		public virtual void setEncrypted()
		{
			properties[ENCRYPTION] = "true";
		}
		
		/// <summary>Method toString</summary>
		public override string ToString()
		{
			System.Text.StringBuilder b = new System.Text.StringBuilder(1024);
			string key, value_Renamed;
			int i = 0;
			
			if (properties.Count == 0)
			{
				return NO_PROPERTIES;
			}
			
			System.Collections.IEnumerator e = properties.Keys.GetEnumerator();
			
			//UPGRADE_TODO: Method 'java.util.Enumeration.hasMoreElements' was converted to 'System.Collections.IEnumerator.MoveNext' which has a different behavior. 'ms-help://MS.VSCC.2003/commoner/redir/redirect.htm?keyword="jlca1073"'
			while (e.MoveNext())
			{
				//UPGRADE_TODO: Method 'java.util.Enumeration.nextElement' was converted to 'System.Collections.IEnumerator.Current' which has a different behavior. 'ms-help://MS.VSCC.2003/commoner/redir/redirect.htm?keyword="jlca1073"'
				key = ((string) e.Current);
				value_Renamed = ((string) properties[key]);
				
				b.Append(LEFT);
				b.Append(i);
				b.Append(RIGHT);
				b.Append(key);
				b.Append(MIDDLE);
				b.Append(value_Renamed);
				b.Append(NEWLINE);
			}
			
			return b.ToString();
		}
	}
}