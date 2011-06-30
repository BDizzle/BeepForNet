/*
* TLSProfile.java  $Revision: 1.1 $ $Date: 2004/09/21 08:47:00 $
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
using org.beepcore.beep.core;
using org.beepcore.beep.profile;
using org.beepcore.beep.transport.tcp;
namespace org.beepcore.beep.profile.tls
{
	
	
	/// <summary> TLS provides encrypted and authenticated communication over
	/// a session. TLS is a tuning profile, a special set of profiles that affect
	/// an entire session. As a result, only one channel with the profile of TLS
	/// may be open per session.
	/// 
	/// </summary>
	/// <seealso cref="#init">
	/// </seealso>
	/// <seealso cref="org.beepcore.beep.profile.Profile">
	/// </seealso>
	/// <seealso cref="org.beepcore.beep.core.Channel">
	/// </seealso>
	public abstract class TLSProfile:TuningProfile, Profile
	{
		/// <summary> factory method that returns an instance the default
		/// implementation.
		/// </summary>
		public static TLSProfile DefaultInstance
		{
			get
			{
				try
				{
					return getInstance("org.beepcore.beep.profile.tls.jsse.TLSProfileJSSE");
				}
				catch (System.ApplicationException e)
				{
				}
				
				try
				{
					return getInstance("org.beepcore.beep.profile.tls.ptls.TLSProfilePureTLSPemInit");
				}
				catch (System.ApplicationException e)
				{
					throw new BEEPException("TLS not installed");
				}
			}
			
		}
		
		/// <summary> default URI of the channel to start to start a TLS channel</summary>
		public const System.String URI = "http://iana.org/beep/TLS";
		
		/// <summary> init sets the criteria for which an TLS connection is made when
		/// a TLS channel is started for a profile.  It should only be
		/// called once.  For the properties, the initiator is defined as
		/// the peer who starts the channel for the TLS profile, the
		/// listener is the peer that receives the the channel start
		/// request, irregardless of which actually started the session.<p>
		/// Each subclass that encapsulates an implementation has its own
		/// properties as to what it needs to initialise.  See the individual
		/// implementations for their properties.
		/// 
		/// </summary>
		/// <param name="uri">used to start a channel with TLS protection
		/// </param>
		/// <param name="config">used to specify the parameters for sessions protected
		/// by this profile's version of TLS.  In other words, if you want another
		/// set of paramters, you must either recall this method or create another
		/// instance of a <code>TLSProfile</code>.
		/// </param>
		abstract public StartChannelListener init(System.String uri, ProfileConfiguration config);
		
		/// <summary> start a channel for the TLS profile.  Besides issuing the
		/// channel start request, it also performs the initiator side
		/// chores necessary to begin encrypted communication using TLS
		/// over a session.  Parameters regarding the type of encryption
		/// and authentication are specified using the profile
		/// configuration passed to the <code>init</code> method Upon
		/// returning, all traffic over the session will be entrusted as
		/// per these parameters.<p>
		/// 
		/// </summary>
		/// <seealso cref="profile configuration">
		/// </seealso>
		/// <param name="session">the session to encrypt communcation for
		/// </param>
		/// <returns> new {@link TCPSession} with TLS negotiated.
		/// @throws BEEPException an error occurs during the channel start
		/// request or the TLS handshake (such as trying to negotiate an
		/// anonymous connection with a peer that doesn't support an
		/// anonymous cipher suite).
		/// </returns>
		abstract public TCPSession startTLS(TCPSession session);
		
		/// <summary> factory method that returns an instance of the implementation
		/// given in the parameter.  As of now, only one is supported,
		/// "PureTLS" returns an instance of TLSProfilePureTLS.  If the
		/// provider is unknown, it tries to load a class of that name and
		/// returns.
		/// </summary>
		/// <param name="provider">implementation to use.
		/// </param>
		public static TLSProfile getInstance(System.String provider)
		{
			try
			{
				//UPGRADE_ISSUE: Class 'java.lang.ClassLoader' was not converted. 'ms-help://MS.VSCC.2003/commoner/redir/redirect.htm?keyword="jlca1000_javalangClassLoader"'
				//UPGRADE_ISSUE: Method 'java.lang.Thread.getContextClassLoader' was not converted. 'ms-help://MS.VSCC.2003/commoner/redir/redirect.htm?keyword="jlca1000"'
				ClassLoader cl = SupportClass.ThreadClass.Current().getContextClassLoader();
				//UPGRADE_ISSUE: Method 'java.lang.ClassLoader.loadClass' was not converted. 'ms-help://MS.VSCC.2003/commoner/redir/redirect.htm?keyword="jlca1000_javalangClassLoader"'
				System.Type whatever = cl.loadClass(provider);
				
				//UPGRADE_TODO: Method 'java.lang.Class.newInstance' was converted to 'SupportClass.CreateNewInstance' which has a different behavior. 'ms-help://MS.VSCC.2003/commoner/redir/redirect.htm?keyword="jlca1073"'
				//UPGRADE_WARNING: Method 'java.lang.Class.newInstance' was converted to 'SupportClass.CreateNewInstance' which may throw an exception. 'ms-help://MS.VSCC.2003/commoner/redir/redirect.htm?keyword="jlca1101"'
				return (TLSProfile) SupportClass.CreateNewInstance(whatever);
			}
			//UPGRADE_NOTE: Exception 'java.lang.ClassNotFoundException' was converted to 'System.Exception' which has different behavior. 'ms-help://MS.VSCC.2003/commoner/redir/redirect.htm?keyword="jlca1100"'
			catch (System.Exception e)
			{
				throw new BEEPException("Provider '" + provider + "' not found.");
			}
			catch (System.Exception e)
			{
				throw new BEEPException(e);
			}
		}
	}
}