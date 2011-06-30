/*
* SessionCredential.java  $Revision: 1.1 $ $Date: 2004/09/21 08:46:56 $
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
	
	
	/// <summary>Class SessionCredential</summary>
	/// <author>Eric Dixon</author>
	/// <author>Huston Franklin</author>
	/// <author>Jay Kint</author>
	/// <author>Scott Pead</author>
	/// <version>$Revision: 1.1 $, $Date: 2004/09/21 08:46:56 $</version>
	public class SessionCredential
	{
		/// <summary>Method getAlgorithm</summary>
		virtual public string Algorithm
		{
			get
			{
				return (string) properties[ALGORITHM];
			}
			
		}
		/// <summary>Method getAuthenticator</summary>
		virtual public string Authenticator
		{
			get
			{
				return (string) properties[AUTHENTICATOR];
			}
			
		}
		/// <summary>Method getAuthorized</summary>
		virtual public string Authorized
		{
			get
			{
				return (string) properties[AUTHORIZED];
			}
			
		}
		/// <summary>Method getAuthenticatorType</summary>
		virtual public string AuthenticatorType
		{
			get
			{
				return (string) properties[AUTHENTICATOR_TYPE];
			}
			
		}
		/// <summary>Method getLocalCertificate</summary>
		virtual public object LocalCertificate
		{
			get
			{
				return properties[LOCAL_CERTIFICATE];
			}
			
		}
		/// <summary>Method getRemoteAddress</summary>
		virtual public string RemoteAddress
		{
			get
			{
				return (string) properties[REMOTE_ADDRESS];
			}
			
		}
		/// <summary>Method getRemoteCertificate</summary>
		virtual public object RemoteCertificate
		{
			get
			{
				return properties[REMOTE_CERTIFICATE];
			}
			
		}
		
		// Constants
		public const string ALGORITHM = "ALGORITHM";
		public const string AUTHENTICATOR = "AUTHENTICATOR";
		public const string AUTHORIZED = "AUTHORIZED";
		public const string AUTHENTICATOR_TYPE = "AUTHENTICATOR_TYPE";
		public const string LOCAL_CERTIFICATE = "LOCAL_CERTIFICATE";
		public const string NO_CREDENTIALS = "Peer has no credentials.";
		public const string REMOTE_ADDRESS = "REMOTE_ADDRESS";
		public const string REMOTE_CERTIFICATE = "REMOTE_CERTIFICATE";
		
		// Pretty Print Crap
		public const string LEFT = "[";
		public const string RIGHT = "]";
		public const string MIDDLE = "=>";
		public const string NEWLINE = "\n";
		
		// Data
		private System.Collections.Hashtable properties;
		
		/// <summary>Constructor SessionCredential</summary>
		public SessionCredential()
		{
			properties = new System.Collections.Hashtable();
		}
		
		/// <summary>Constructor SessionCredential</summary>
		/// <param name="props"></param>
		public SessionCredential(System.Collections.Hashtable props)
		{
			properties = props;
		}
		
		/// <summary>Method toString</summary>
		public override string ToString()
		{
			System.Text.StringBuilder b = new System.Text.StringBuilder(1024);
			string key, value_Renamed;
			int i = 0;
			
			if (properties.Count == 0)
			{
				return NO_CREDENTIALS;
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