/*
* ChannelPool.java  $Revision: 1.1 $ $Date: 2004/09/21 08:46:57 $
*
* Copyright (c) 2001 Invisible Worlds, Inc.  All rights reserved.
* Copyright (c) 2002,2003 Huston Franklin.  All rights reserved.
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
using System.Collections;
using beepcore.beep.core;
using beepcore.logging;

namespace beepcore.beep.lib
{
	/// <summary><code>ChannelPool</code> holds a collection of available
	/// <code>SharedChannel</code>(s) and provides access to them.  Availabe
	/// <code>SharedChannel</code>(s) are retrieved through the
	/// <code>getSharedChannel</code>.  Each <code>SharedChannel</code> has a time
	/// to live, after which, it is removed from the pool.</summary>
	/// <author>Eric Dixon</author>
	/// <author>Huston Franklin</author>
	/// <author>Jay Kint</author>
	/// <author>Scott Pead</author>
	/// <version>$Revision: 1.1 $, $Date: 2004/09/21 08:46:57 $</version>
	public class ChannelPool
	{
		/// <summary>Sets the time to live or the number of milleseconds an unused channel will
		/// remain in the pool before it is removed from the pool.
		/// The default time to live is two minutes.</summary>
		/// <param name="ttl">The time this channel has to live in the pool while in an
		/// available state.</param>
		virtual public long SharedChannelTTL
		{
			set
			{
				this.timeToLive = value;
			}
			
		}
		
		/// <summary>@todo add tuning mechanisms, try to figure out what the average
		/// channel reuse is and adjust ttl accordingly</summary>
		private const long DEFAULT_TIME_TO_LIVE = 120000; // two minutes
		
		private Log log;
		private long timeToLive;
		private ISession session;
		private ArrayList availableChannels;
		
		/// <summary>Creates a <code>ChannelPool</code> with the given session.</summary>
		/// <param name="session">The <code>session</code> on which all
		/// <code>SharedChannel</code>(s) returned from <code>getSharedChannel</code>
		/// are started.</param>
		/// <seealso cref="#getSharedChannel"></seealso>
		public ChannelPool(ISession session) : this(session, DEFAULT_TIME_TO_LIVE)
		{
		}
		
		/// <summary>Creates a <code>ChannelPool</code> with the given session and time to
		/// live.</summary>
		/// <param name="session">The <code>session</code> on which all
		/// <code>SharedChannel</code>(s) returned from <code>getSharedChannel</code>
		/// are started.</param>
		/// <param name="ttl">The time in milleseconds an available <code>SharedChannel</code>
		/// will live in <code>ChannelPool</code></param>
		/// <seealso cref="#getSharedChannel"></seealso>
		public ChannelPool(ISession session, long ttl)
		{
			this.log = LogFactory.getLog(this.GetType());
			this.availableChannels = new ArrayList();
			this.session = session;
			this.timeToLive = ttl;
		}
		
		/// <summary>Returns a <code>SharedChannel</code> which supports the specified
		/// <code>profile</code>.</summary>
		/// <param name="profile">Name of a profile for the requested
		/// <code>SharedChannel</code>.</param>
		/// <returns>A <code>SharedChannel</code> for the requested profile.</returns>
		/// <exception cref="BEEPException" />
		public virtual SharedChannel getSharedChannel(string profile)
		{
			SharedChannel sharedCh = null;
			bool found = false;
			
			lock (availableChannels)
			{
				for(int i=0;i<availableChannels.Count;i++) 
				{
					sharedCh = (SharedChannel) availableChannels[i];
					
					if (sharedCh.getProfile().Equals(profile))
					{
						log.trace("Found an available channel for sharing");

						availableChannels.RemoveAt(i);
						
						found = true;
						
						break;
					}
				}
			}
			
			// nothing found, so create one and return it
			if (!found)
			{
				sharedCh = new SharedChannel(this.session.startChannel(profile), this);
			}
			
			// clean up channels that have expired
			garbageCollect();
			if (log.isTraceEnabled())
			{
				log.trace("Sharing channel number:" + sharedCh.Number);
			}
			
			return sharedCh;
		}
		
#if MESSAGELISTENER
		/// <summary>Returns a <code>SharedChannel</code> which supports the specified
		/// <code>profile</code> and calls back on the specified
		/// <code>DataListener</code>.  Once it is no longer needed, call
		/// <code>release</code> on the <code>SharedChannel</code>
		/// to return it to the pool of available channels.</summary>
		/// <param name="profile">Name of profile for the requested
		/// <code>SharedChannel</code>.</param>
		/// <param name="listener"><code>DataListener</code> for the requested
		/// <code>SharedChannel</code>.</param>
		/// <returns>A <code>SharedChannel</code>.</returns>
		/// <seealso cref="MessageListener"></seealso>
		/// <seealso cref="SharedChannel"></seealso>
		/// <exception cref="BEEPException" />
		/// <deprecated></deprecated>
		public virtual SharedChannel getSharedChannel(string profile, MessageListener listener)
		{
			lock (this)
			{
				SharedChannel sharedCh = null;
				bool found = false;
				
				lock (availableChannels)
				{
					for(int i=0;i<availableChannels.Count;i++) 
					{
						sharedCh = (SharedChannel) availableChannels[i];
					
						if (sharedCh.getProfile().Equals(profile))
						{
							log.trace("Found an available channel for sharing");

							availableChannels.RemoveAt(i);
						
							found = true;
						
							break;
						}
					}
				}
				
				// nothing found, so create one and return it
				if (!found)
				{
					sharedCh = new SharedChannel(this.session.startChannel(profile, listener), this);
				}
				
				// clean up channels that have expired
				garbageCollect();
				if (log.isTraceEnabled())
				{
					log.trace("Sharing channel number:" + sharedCh.Number);
				}
				
				return sharedCh;
			}
		}
#endif

		/// <summary>Returns a <code>SharedChannel</code> which supports the specified
		/// <code>profile</code> and calls back on the specified
		/// <code>IRequestHandler</code>.  Once it is no longer needed, call
		/// <code>release</code> on the <code>SharedChannel</code>
		/// to return it to the pool of available channels.</summary>
		/// <param name="profile">Name of profile for the requested
		/// <code>SharedChannel</code>.</param>
		/// <param name="handler"><code>IRequestHandler</code> for the requested
		/// <code>SharedChannel</code>.</param>
		/// <returns>A <code>SharedChannel</code>.</returns>
		/// <seealso cref="IRequestHandler"></seealso>
		/// <seealso cref="SharedChannel"></seealso>
		/// <exception cref="BEEPException" />
		/// <deprecated></deprecated>
		public virtual SharedChannel getSharedChannel(string profile, IRequestHandler handler)
		{
			lock (this)
			{
				SharedChannel sharedCh = null;
				bool found = false;
				
				lock (availableChannels)
				{
					for(int i=0;i<availableChannels.Count;i++) 
					{
						sharedCh = (SharedChannel) availableChannels[i];
					
						if (sharedCh.getProfile().Equals(profile))
						{
							log.trace("Found an available channel for sharing");

							availableChannels.RemoveAt(i);
						
							found = true;
						
							break;
						}
					}
				}
				
				// nothing found, so create one and return it
				if (!found)
				{
					sharedCh = new SharedChannel(this.session.startChannel(profile, handler), this);
				}
				
				// clean up channels that have expired
				garbageCollect();
				if (log.isTraceEnabled())
				{
					log.trace("Sharing channel number:" + sharedCh.Number);
				}
				
				return sharedCh;
			}
		}

		/// <summary>Called from <code>SharedChannel</code>.  Releases the sharedCh and adds
		/// it to the list of available SharedChannels.</summary>
		internal virtual void releaseSharedChannel(SharedChannel sharedCh)
		{
			
			// this channel is available to share
			lock (availableChannels)
			{
				availableChannels.Add(sharedCh);
			}
			
			garbageCollect();
		}
		
		/// <summary>Closes down the channel pool, its session and all associated channels.</summary>
		public virtual void close()
		{
			// close all available channels and the session
			try
			{
				this.session.close();
			}
			catch (BEEPException e)
			{
				Console.Error.WriteLine(e.ToString());
			}
		}
		
		/// <summary>Identifies SharedChannels that have exceeded their ttl, removes the from
		/// the list of availableChannels, and closes them.</summary>
		private void garbageCollect()
		{
			log.trace("garbage collecting");
			
			if (availableChannels.Count != 0)
			{
				//UPGRADE_TODO: Method 'java.util.Date.getTime' was converted to 'System.DateTime.Ticks' which has a different behavior. 'ms-help://MS.VSCC.2003/commoner/redir/redirect.htm?keyword="jlca1073"'
				long ttl = System.DateTime.Now.Ticks - timeToLive;
				
				lock (availableChannels)
				{
					while (availableChannels.Count > 0)
					{
						if (ttl > ((SharedChannel) availableChannels[0]).TTL)
						{
							
							// channel has out lived its time to live
							// die, die, die
							SharedChannel shCh = (SharedChannel) availableChannels[0];
							availableChannels.RemoveAt(0);
							
							try
							{
								if (log.isTraceEnabled())
								{
									log.trace("garbage collected channel number:" + shCh.Number);
								}
								shCh.close(); // last gasp
							}
							catch (BEEPException)
							{
								// ignore for now, we'll try again later
								log.error("unable to close channel number:" + shCh.Number);
							}
							
							shCh = null; // death by gib @todo does this really set the object to null?
						}
						else
						{
							
							// since youngest channels are added to the end of the list,
							// once we hit the above condition all channels below this one are
							break;
						}
					}
				}
			}
		}
	}
}