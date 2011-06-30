/*
* SharedChannel.java            $Revision: 1.1 $ $Date: 2004/09/21 08:46:57 $
*
* Copyright (c) 2001 Invisible Worlds, Inc.  All rights reserved.
* Copyright (c) 2002, 2003 Huston Franklin.  All rights reserved.
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
using beepcore.beep.core;

namespace beepcore.beep.lib
{
	
	/// <summary><code>SharedChannel</code> references a <code>Channel</code>.  A
	/// <code>ChannelPool</code> holds a collection of <code>SharedChannel</code>s.
	/// Call <code>ChannelPoll</code>'s <code>getSharedChannel</code> to create
	/// a <code>SharedChannel</code>.</summary>
	/// <seealso cref="ChannelPool"></seealso>
	/// <author>Eric Dixon</author>
	/// <author>Huston Franklin</author>
	/// <author>Jay Kint</author>
	/// <author>Scott Pead</author>
	/// <version>$Revision: 1.1 $, $Date: 2004/09/21 08:46:57 $</version>
	public class SharedChannel : IChannel
	{
		virtual internal long TTL
		{
			get
			{
				return this.timeStamp;
			}
			
		}
		virtual public int Number
		{
			get
			{
				return channel.Number;
			}
			
		}
		virtual public ISession Session
		{
			get
			{
				return channel.Session;
			}
			
		}
		virtual public string StartData
		{
			get
			{
				return channel.StartData;
			}
			
			set
			{
				channel.StartData = value;
			}
			
		}
		virtual public object AppData
		{
			get
			{
				return channel.AppData;
			}
			
			set
			{
				channel.AppData = value;
			}
			
		}
		
		private IChannel channel = null; // Channel this SharedChannel references
		private ChannelPool pool = null; // the ChannelPool this SharedChannel belongs to
		private long timeStamp = - 1; // time SharedChannel became available
		
		/// <summary>Creates a <code>SharedChannel</code> with given <code>channel</code>
		/// and <code>pool</code></summary>
		/// <param name="channel">The channel this object references.</param>
		/// <param name="pool">The <code>ChannelPool</code> this object belongs to.</param>
		/// <seealso cref="ChannelPool"></seealso>
		internal SharedChannel(IChannel channel, ChannelPool pool)
		{
			this.channel = channel;
			this.pool = pool;
		}
		
		/// <summary>Marks this <code>SharedChannel</code> as available for reuse.</summary>
		public virtual void  release()
		{
			System.DateTime now = System.DateTime.Now;
			
			//UPGRADE_TODO: Method 'java.util.Date.getTime' was converted to 'System.DateTime.Ticks' which has a different behavior. 'ms-help://MS.VSCC.2003/commoner/redir/redirect.htm?keyword="jlca1073"'
			this.timeStamp = now.Ticks;
			
			this.pool.releaseSharedChannel(this);
		}
		
		/// <summary>Send a message of type MSG. Sends <code>stream</code> as message's
		/// payload.</summary>
		/// <param name="stream"><code>DataStream</code> that is read to send data.</param>
		/// <param name="replyListener"></param>
		/// <returns>MessageStatus Can be queried to get status information about the
		/// message.</returns>
		/// <seealso cref="OutputDataStream"></seealso>
		/// <seealso cref="MessageStatus"></seealso>
		/// <exception cref="BEEPException" />
		public virtual MessageStatus sendMSG(OutputDataStream stream, ReplyListener replyListener)
		{
			return channel.sendMSG(stream, replyListener);
		}
		
#if MESSAGELISTENER
		/// <summary>Sets the <code>DataListener</code> for this <code>SharedChannel</code>.</summary>
		/// <param name="ml">A listener of type <code>DataListener</code></param>
		public virtual MessageListener setMessageListener(MessageListener ml)
		{
			return channel.setMessageListener(ml);
		}
		
		public virtual MessageListener getMessageListener()
		{
			return channel.getMessageListener();
		}
#endif

		public virtual IRequestHandler getRequestHandler()
		{
			return channel.getRequestHandler();
		}
		
		public virtual IRequestHandler setRequestHandler(IRequestHandler handler)
		{
			return channel.setRequestHandler(handler);
		}
		
		public virtual IRequestHandler setRequestHandler(IRequestHandler handler, bool tuningReset)
		{
			return channel.setRequestHandler(handler, tuningReset);
		}
		
		public virtual core.ChannelState getState()
		{
			return channel.getState();
		}
		
		/// <summary>Returns the profile used to create this <code>SharedChannel</code>.</summary>
		public virtual string getProfile()
		{
			return channel.getProfile();
		}
		
		/// <summary>Closes this <code>SharedChannel</code>.</summary>
		/// <exception cref="BEEPException" />
		public virtual void  close()
		{
			channel.close();
		}
		
		/// <summary>Sends a 'synchronous' request on this <code>SharedChannel</code>.</summary>
		/// <param name="ds"><code>DataStream</code> to send as this request's payload.</param>
		/// <returns>Reply Caller may block using this object to retrieve the reply
		/// to this request.</returns>
		/// <exception cref="BEEPException" />
		public virtual Reply sendRequest(OutputDataStream ds)
		{
			Reply r = new Reply();
			channel.sendMSG(ds, r);
			return r;
		}
	}
}