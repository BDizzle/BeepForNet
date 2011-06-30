/*
* ProfileRegistry.java  $Revision: 1.1 $ $Date: 2004/09/21 08:46:56 $
*
* Copyright (c) 2001 Invisible Worlds, Inc.  All rights reserved.
* Copyright (c) 2001,2002 Huston Franklin.  All rights reserved.
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
using Log = beepcore.logging.Log;
using LogFactory = beepcore.logging.LogFactory;
namespace beepcore.beep.core
{
	
	
	/// <summary>Maintains a set of associations between URIs and
	/// <code>StartChannelListener</code>s. This set is used to generate
	/// the <code>greeting</code> and to demux <code>start</code> requests.</summary>
	/// <author>Eric Dixon</author>
	/// <author>Huston Franklin</author>
	/// <author>Jay Kint</author>
	/// <author>Scott Pead</author>
	/// <version>$Revision: 1.1 $, $Date: 2004/09/21 08:46:56 $</version>
	public class ProfileRegistry : System.ICloneable
	{
		private void  InitBlock()
		{
			log = LogFactory.getLog(this.GetType());
		}
		/// <summary>Returns the currently registered profile URIs.</summary>
		virtual public System.Collections.IEnumerator Profiles
		{
			get
			{
				return profileListeners.Keys.GetEnumerator();
			}
			
		}
		//UPGRADE_NOTE: Respective javadoc comments were merged.  It should be changed in order to comply with .NET documentation conventions.
		/// <summary>Returns the value for the BEEP greeting localize attribute.</summary>
		/// <summary>Set values for the BEEP greeting localize attribute.</summary>
		/// <param name="localize"></param>
		virtual public string Localization
		{
			get
			{
				return this.localize;
			}
			
			set
			{
				this.localize = value;
			}
			
		}
		
		// Instance Data
		//UPGRADE_NOTE: The initialization of  'log' was moved to method 'InitBlock'. 'ms-help://MS.VSCC.2003/commoner/redir/redirect.htm?keyword="jlca1005"'
		private Log log;
		
		//UPGRADE_NOTE: Field 'EnclosingInstance' was added to class 'InternalProfile' to access its enclosing instance. 'ms-help://MS.VSCC.2003/commoner/redir/redirect.htm?keyword="jlca1019"'
		private class InternalProfile
		{
			public InternalProfile(ProfileRegistry enclosingInstance)
			{
				InitBlock(enclosingInstance);
			}
			private void  InitBlock(ProfileRegistry enclosingInstance)
			{
				this.enclosingInstance = enclosingInstance;
			}
			private ProfileRegistry enclosingInstance;
			public ProfileRegistry Enclosing_Instance
			{
				get
				{
					return enclosingInstance;
				}
				
			}
			internal IStartChannelListener listener;
			internal SessionTuningProperties tuning;
		}
		
		private System.Collections.Hashtable profileListeners;
		internal string localize;
		
		// Constructors
		
		/// <summary>Constructor ProfileRegistry</summary>
		public ProfileRegistry()
		{
			InitBlock();
			this.localize = core.StringConstants.LOCALIZE_DEFAULT;
			this.profileListeners = new System.Collections.Hashtable();
		}
		
		private ProfileRegistry(string localize, System.Collections.Hashtable profiles)
		{
			InitBlock();
			this.localize = localize;
			this.profileListeners = profiles;
		}
		
		//UPGRADE_TODO: The equivalent of method 'java.lang.Object.clone' is not an override method. 'ms-help://MS.VSCC.2003/commoner/redir/redirect.htm?keyword="jlca1143"'
		public object Clone()
		{
			return new ProfileRegistry(this.localize, (System.Collections.Hashtable) this.profileListeners.Clone());
		}
		
		/// <summary>Returns the <code>StartChannelListener</code> for the specified URI.</summary>
		/// <param name="uri"></param>
		public virtual IStartChannelListener getStartChannelListener(SessionTuningProperties tuning, string uri)
		{
			
			InternalProfile profile = (InternalProfile) profileListeners[uri];
			
			if (profile == null)
			{
				return null;
			}
			
			// if there are no qualifications, then just return the listener
			if (profile.tuning == null || profile.tuning.Empty)
			{
				return ((InternalProfile) profileListeners[uri]).listener;
			}
			
			// so the profile requires something, but if the session doesn't
			// have anything, then return null
			if (tuning == null)
			{
				log.debug("Session does not have any tuning properties");
				return null;
			}
			
			// if the profile requires any of the standard properties, then
			// make sure they are set on the session before returning the listener
			int i = 0;
			
			for (i = 0; i < SessionTuningProperties.STANDARD_PROPERTIES.Length; i++)
			{
				if ((profile.tuning.getProperty(SessionTuningProperties.STANDARD_PROPERTIES[i]) != null) && (tuning.getProperty(SessionTuningProperties.STANDARD_PROPERTIES[i]) == null))
				{
					if (log.isDebugEnabled())
					{
						log.debug("Session does not have tuning property " + SessionTuningProperties.STANDARD_PROPERTIES[i]);
					}
					return null;
				}
			}
			
			// all the ones the profile requested must be there so we return the
			// listener
			return ((InternalProfile) profileListeners[uri]).listener;
		}
		
		/// <summary>Add the <code>StartChannelListener</code> for the specified URI.</summary>
		/// <param name="profile"></param>
		/// <param name="listener"></param>
		/// <returns>the previously registered <code>StartChannelListener</code></returns>
		public virtual IStartChannelListener addStartChannelListener(string profile, IStartChannelListener listener, SessionTuningProperties tuning)
		{
			lock (this)
			{
				// Replace semantics - change this if we want to prevent clobbering.
				IStartChannelListener temp = null;
				
				if (profileListeners[profile] != null)
				{
					temp = ((InternalProfile) profileListeners[profile]).listener;
				}
				
				InternalProfile tempProfile = new InternalProfile(this);
				
				tempProfile.listener = listener;
				
				tempProfile.tuning = tuning;
				
				profileListeners[profile] = tempProfile;
				
				return temp;
			}
		}
		
		/// <summary>Remove the <code>StartChannelListener</code> for the specified uri.</summary>
		/// <param name="profile"></param>
		/// <returns><code>StartChannelListener</code> registered for the specified
		/// uri.</returns>
		public virtual IStartChannelListener removeStartChannelListener(string profile)
		{
			lock (this)
			{
				InternalProfile temp = (InternalProfile) profileListeners[profile];

				profileListeners.Remove(profile);
				
				return temp.listener;
			}
		}
		
		internal virtual byte[] getGreeting(ISession session)
		{
			return getGreeting(session, null);
		}
		
		internal virtual byte[] getGreeting(ISession session, string features)
		{
			int bufferSize = "<greeting></greeting>".Length;
			int profileCount = 0;
			
			profileCount = profileListeners.Count;
			
			System.Collections.IEnumerator e = profileListeners.Keys.GetEnumerator();
			
			//UPGRADE_TODO: Method 'java.util.Enumeration.hasMoreElements' was converted to 'System.Collections.IEnumerator.MoveNext' which has a different behavior. 'ms-help://MS.VSCC.2003/commoner/redir/redirect.htm?keyword="jlca1073"'
			while (e.MoveNext())
			{
				//UPGRADE_TODO: Method 'java.util.Enumeration.nextElement' was converted to 'System.Collections.IEnumerator.Current' which has a different behavior. 'ms-help://MS.VSCC.2003/commoner/redir/redirect.htm?keyword="jlca1073"'
				bufferSize += ((string) e.Current).Length + "<profile>".Length;
			}
			
			bufferSize++;
			
			System.Text.StringBuilder sb = new System.Text.StringBuilder(bufferSize);
			
			// Create Greeting
			// Wish I could reset these.
			System.Collections.IEnumerator f = profileListeners.Keys.GetEnumerator();
			
			sb.Append("<greeting");
			
			if (((object) localize != null) && !localize.Equals(core.StringConstants.LOCALIZE_DEFAULT))
			{
				sb.Append(" localize='");
				sb.Append(localize);
				sb.Append('\'');
			}
			
			if ((object) features != null)
			{
				sb.Append(" features='");
				sb.Append(features);
				sb.Append('\'');
			}
			
			sb.Append('>');
			
			//UPGRADE_TODO: Method 'java.util.Enumeration.hasMoreElements' was converted to 'System.Collections.IEnumerator.MoveNext' which has a different behavior. 'ms-help://MS.VSCC.2003/commoner/redir/redirect.htm?keyword="jlca1073"'
			while (f.MoveNext())
			{
				
				// make sure this profile wants to be advertised
				try
				{
					//UPGRADE_TODO: Method 'java.util.Enumeration.nextElement' was converted to 'System.Collections.IEnumerator.Current' which has a different behavior. 'ms-help://MS.VSCC.2003/commoner/redir/redirect.htm?keyword="jlca1073"'
					string profileName = (string) f.Current;
					InternalProfile profile = (InternalProfile) profileListeners[profileName];
					bool callAdvertise = false;
					SessionTuningProperties sessionTuning = session.getTuningProperties();
					
					// check the standard tuning settings first
					for (int i = 0; i < SessionTuningProperties.STANDARD_PROPERTIES.Length; i++)
					{
						
						if ((profile.tuning != null) && (sessionTuning != null) && (profile.tuning.getProperty(SessionTuningProperties.STANDARD_PROPERTIES[i]) != null) && (sessionTuning.getProperty(SessionTuningProperties.STANDARD_PROPERTIES[i]) != null))
						{
							callAdvertise = true;
						}
					}
					
					if ((profile.tuning == null) || (callAdvertise && profile.listener.AdvertiseProfile(session)))
					{
						sb.Append("<profile uri='");
						sb.Append(profileName);
						sb.Append("' />");
					}
				}
				catch (BEEPException x)
				{
					Console.Error.WriteLine(x.ToString());
					
					continue;
				}
			}
			
			sb.Append("</greeting>");
			
			return System.Text.Encoding.ASCII.GetBytes(sb.ToString());
		}
	}
}