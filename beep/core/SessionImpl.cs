/*
* SessionImpl.java  $Revision: 1.1 $ $Date: 2004/09/21 08:46:56 $
*
* Copyright (c) 2001 Invisible Worlds, Inc.  All rights reserved.
* Copyright (c) 2001-2003 Huston Franklin.  All rights reserved.
* Copyright (c) 2002 Kevin Kress.  All rights reserved.
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

using Log = beepcore.logging.Log;
using LogFactory = beepcore.logging.LogFactory;

using beepcore.beep.core.@event;

using System.Xml;

namespace beepcore.beep.core
{
	
	
	/// <summary>This class encapsulates the notion of a BEEP Session (a relationship
	/// between BEEP peers).
	/// <p>
	/// The implementor should sub-class <code>Session</code>'s abstract methods
	/// for a given transport.
	/// It's principal function is to sit on whatever network or referential
	/// 'connection' exists between the BEEP peers, read and write BEEP frames and
	/// deliver them to or receive them from the associated <code>Channel</code>.
	/// 
	/// @todo Improvments could include sharing buffers from DataStream to here,
	/// minimizing stream writes/reads, and pooling I/O threads.</summary>
	/// <author>Eric Dixon</author>
	/// <author>Huston Franklin</author>
	/// <author>Jay Kint</author>
	/// <author>Scott Pead</author>
	/// <version>$Revision: 1.1 $, $Date: 2004/09/21 08:46:56 $</version>
	/// <seealso cref="Channel"></seealso>
	public abstract class SessionImpl : ISession
	{
		/// <summary>Returns the profiles sent by the remote peer in the greeting.</summary>

		virtual public System.Collections.ICollection PeerSupportedProfiles
		{
			get
			{
				return this.peerSupportedProfiles;
			}
			
		}
		/// <summary>Returns the <code>ProfileRegistry</code> for <code>Session</code>.</summary>
		/// <returns>A <code>ProfileRegistry</code>.</returns>
		/// <seealso cref="ProfileRegistry"></seealso>
		virtual public ProfileRegistry ProfileRegistry
		{
			get
			{
				return profileRegistry;
			}
			
		}
		/// <summary>Returns the state of <code>Session</code>.</summary>
		/// <returns>Session state (see the Constants in this class).</returns>
		virtual public core.SessionState State
		{
			get
			{
				return this.state;
			}
			
		}
		/// <summary>Indicates whehter or not this session is in the initiator role.</summary>
		virtual public bool Initiator
		{
			get
			{
				return ((nextChannelNumber % 2) == 1);
			}
			
		}
		/// <summary>Returns the maximum frame size that a channel should send for
		/// this session.
		/// </summary>
		/// <exception cref="BEEPException" />
		protected internal abstract int MaxFrameSize{get;}

		virtual public string ServerName
		{
			get
			{
				return serverName;
			}
			
		}
		/// <summary>Returns the next free channel number as a string.</summary>
		/// <returns>Channel number.</returns>
		private string NextFreeChannelNumber
		{
			get
			{
				lock (this)
				{
					long i;
					
					lock (this)
					{
						
						// next channel needs to be incremented by two since the peer
						// uses every other channel (see beep spec)
						i = nextChannelNumber;
						nextChannelNumber += 2;
					}
					
					string nextChannel = System.Convert.ToString(i);
					
					if (overflow)
					{
						
						// Equally insane collision check after the rollover
						if (channels[nextChannel] != null)
						{
							return NextFreeChannelNumber;
						}
					}
					
					// Insane bounds check that will probably never happen
					if (nextChannelNumber > Frame.MAX_CHANNEL_NUMBER)
					{
						nextChannelNumber = nextChannelNumber % Frame.MAX_CHANNEL_NUMBER;
						overflow = true;
					}
					
					// Warning: nextChannelNumber is a long to detect overflow
					return nextChannel;
				}
			}
			
		}
		private string StateString
		{
			get
			{
				switch (state)
				{
					
					case core.SessionState.SESSION_STATE_INITIALIZED: 
						return "initialized";
					
					case core.SessionState.SESSION_STATE_GREETING_SENT: 
						return "greeting sent";
					
					case core.SessionState.SESSION_STATE_ACTIVE: 
						return "active";
					
					case core.SessionState.SESSION_STATE_TUNING_PENDING: 
						return "tuning pending";
					
					case core.SessionState.SESSION_STATE_TUNING: 
						return "tuning";
					
					case core.SessionState.SESSION_STATE_CLOSE_PENDING: 
						return "close pending";
					
					case core.SessionState.SESSION_STATE_CLOSING: 
						return "closing";
					
					case core.SessionState.SESSION_STATE_CLOSED: 
						return "closed";
					
					case core.SessionState.SESSION_STATE_ABORTED: 
						return "aborted";
					
					default: 
						return "unknown";
					
				}
			}
			
		}
		
		// Constants
		private static readonly SessionOperations[] ops = new SessionOperations[]
		{
			new INITIALIZED_SessionOperations(),
			new GREETING_SENT_SessionOperations(),
			new ACTIVE_SessionOperations(),
			new TUNING_PENDING_SessionOperations(),
			new TUNING_SessionOperations(),
			new CLOSE_PENDING_SessionOperations(),
			new CLOSING_SessionOperations(),
			new CLOSED_SessionOperations(),
			new ABORTED_SessionOperations()
		};
		
		private const int DEFAULT_CHANNELS_SIZE = 4;
		private const int DEFAULT_PROPERTIES_SIZE = 4;
		private const int DEFAULT_POLL_INTERVAL = 500;
		
		/// <summary>@todo check this </summary>
		internal const int MAX_PCDATA_SIZE = 4096;
		private const int MAX_START_CHANNEL_WAIT = 60000;
		private const int MAX_START_CHANNEL_INTERVAL = 100;
		
		private const string CHANNEL_ZERO = "0";
		
		private const string ERR_MALFORMED_XML_MSG = "Malformed XML";
		private const string ERR_UNKNOWN_OPERATION_ELEMENT_MSG = "Unknown operation element";
		
		private static readonly byte[] OK_ELEMENT = System.Text.Encoding.ASCII.GetBytes("<ok />");
		
		// Instance Data
		//UPGRADE_NOTE: The initialization of  'log' was moved to method 'InitBlock'. 'ms-help://MS.VSCC.2003/commoner/redir/redirect.htm?keyword="jlca1005"'
		private Log log;
		
		private core.SessionState state;
		private long nextChannelNumber = 0;
		private ChannelImpl zero;
		private System.Collections.Hashtable channels = null;
		private System.Collections.Hashtable properties = null;

		private ArrayList sessionListeners = ArrayList.Synchronized(new ArrayList());
		
		private ArrayList channelListeners = ArrayList.Synchronized(new ArrayList());

		private ProfileRegistry profileRegistry = null;
		private SessionCredential localCredential, peerCredential;
		private SessionTuningProperties tuningProperties = null;

		private System.Collections.ICollection peerSupportedProfiles = null;
		
		private bool overflow;
		//private bool allowChannelWindowUpdates = true;

		private string serverName;
		private bool sentServerName = false;
		
		/// <summary>Default Session Constructor.  A relationship between peers - a session -
		/// consists of a set of profiles they share in common, and an ordinality
		/// (to prevent new channel collision) so that the initiator starts odd
		/// channels and the listener starts channels with even numbers.</summary>
		/// <param name="registry">The Profile Registry summarizing the profiles this
		/// Session will support</param>
		/// <param name="firstChannel">used internally in the API, an indication of the
		/// ordinality of the channels this peer can start, odd, or even.</param>
		/// <param name="localCred"></param>
		/// <param name="peerCred"></param>
		/// <param name="tuning"></param>
		/// <param name="serverName"></param>
		protected internal SessionImpl(ProfileRegistry registry, int firstChannel, SessionCredential localCred, SessionCredential peerCred, SessionTuningProperties tuning, string serverName)
		{
			log = LogFactory.getLog(this.GetType());

			state = core.SessionState.SESSION_STATE_INITIALIZED;
			localCredential = localCred;
			peerCredential = peerCred;
			nextChannelNumber = firstChannel;
			overflow = false;
			profileRegistry = registry;
			channels = new System.Collections.Hashtable(DEFAULT_CHANNELS_SIZE);
			properties = new System.Collections.Hashtable(DEFAULT_PROPERTIES_SIZE);
			tuningProperties = tuning;
			this.serverName = serverName;
		}
		
		/// <summary>Initializes the <code>Session</code>.  Initializes Channel Zero and its
		/// listener. Sends a greeting and waits for corresponding greeting.</summary>
		/// <exception cref="BEEPException" />
		protected internal virtual void  init()
		{
			this.peerSupportedProfiles = null;
			
			GreetingListener greetingListener = new GreetingListener(this);
			
#if MESSAGELISTENER
			zero = ChannelImpl.createChannelZero(this, greetingListener, new ChannelImpl.MessageListenerAdapter(new ChannelZeroListener(this)));
#else
			zero = ChannelImpl.createChannelZero(this, greetingListener, new ChannelZeroListener(this));
#endif
			
			channels[CHANNEL_ZERO] = zero;
			
			// send greeting
			sendGreeting();
			changeState(core.SessionState.SESSION_STATE_GREETING_SENT);
			
			// start our listening thread we can now receive a greeting
			this.enableIO();
			
			// blocks until greeting is received or MAX_GREETING_WAIT is reached
			int waitCount = 0;
			
			while ((state < core.SessionState.SESSION_STATE_ACTIVE) && (waitCount < MAX_START_CHANNEL_WAIT))
			{
				try
				{
					lock (greetingListener)
					{
						
						//zero.wait(MAX_START_CHANNEL_INTERVAL);
						System.Threading.Monitor.Wait(greetingListener, TimeSpan.FromMilliseconds(MAX_START_CHANNEL_INTERVAL));
						
						waitCount += MAX_START_CHANNEL_INTERVAL;
					}
				}
				catch (System.Threading.ThreadInterruptedException)
				{
					waitCount += MAX_START_CHANNEL_INTERVAL;
				}
			}
			
			// check the channel state and return the appropriate exception
			if (state != core.SessionState.SESSION_STATE_ACTIVE)
			{
				throw new BEEPException("Greeting exchange failed");
			}
		}
		
		/// <summary>A reentrant version of init() that doesn't block the
		/// first I/O thread waiting on a greeting when it should die
		/// and go away.</summary>
		/// <exception cref="BEEPException" />
		protected internal virtual void  tuningInit()
		{
			log.debug("Session.tuningInit");
			
			this.peerSupportedProfiles = null;
			
			GreetingListener greetingListener = new GreetingListener(this);
			
#if MESSAGELISTENER
			zero = ChannelImpl.createChannelZero(this, greetingListener, new ChannelImpl.MessageListenerAdapter(new ChannelZeroListener(this)));
#else
			zero = ChannelImpl.createChannelZero(this, greetingListener, new ChannelZeroListener(this));
#endif
			channels[CHANNEL_ZERO] = zero;
			
			// send greeting
			sendGreeting();
			changeState(core.SessionState.SESSION_STATE_GREETING_SENT);
			
			// start our listening thread we can now receive a greeting
			this.enableIO();
		}
		
		/// <summary>adds the listener from the list of listeners to be notified
		/// of future events.</summary>
		/// <seealso cref="#removeChannelListener"></seealso>
		public virtual void  addChannelListener(IChannelListener l)
		{
			channelListeners.Add(l);
		}
		
		/// <summary>adds the listener from the list of listeners to be notified
		/// of future events.</summary>
		/// <seealso cref="#removeSessionListener"></seealso>
		public virtual void  addSessionListener(ISessionListener l)
		{
			sessionListeners.Add(l);
		}
		
		/// <summary>Closes the <code>Session</code> gracefully. The profiles for
		/// the open channels on the session may veto the close request.</summary>
		/// <exception cref="BEEPException" />
		public virtual void  close()
		{
			if (log.isDebugEnabled())
			{
				log.debug("Closing Session with " + channels.Count + " channels");
			}
			
			changeState(core.SessionState.SESSION_STATE_CLOSE_PENDING);
			
			System.Collections.IEnumerator i = channels.Values.GetEnumerator();
			
			//UPGRADE_TODO: Method 'java.util.Iterator.hasNext' was converted to 'System.Collections.IEnumerator.MoveNext' which has a different behavior. 'ms-help://MS.VSCC.2003/commoner/redir/redirect.htm?keyword="jlca1073_javautilIteratorhasNext"'
			while (i.MoveNext())
			{
				//UPGRADE_TODO: Method 'java.util.Iterator.next' was converted to 'System.Collections.IEnumerator.Current' which has a different behavior. 'ms-help://MS.VSCC.2003/commoner/redir/redirect.htm?keyword="jlca1073_javautilIteratornext"'
				ChannelImpl ch = (ChannelImpl) i.Current;
				
				// if this channel is not zero, call the channel's scl
				if (ch.Number == 0)
				{
					continue;
				}
				
				IStartChannelListener scl = profileRegistry.getStartChannelListener(this.tuningProperties, ch.getProfile());
				
				if (scl == null)
				{
					continue;
				}
				
				// check locally first to see if it is ok to close the channel
				try
				{
					scl.CloseChannel(ch);
				}
				catch (CloseChannelException cce)
				{
					changeState(core.SessionState.SESSION_STATE_ACTIVE);
					// @todo rollback notification
					throw new BEEPException("Close Session rejected by local " + "channel " + ch.getProfile(),cce);
				}
			}
			
			changeState(core.SessionState.SESSION_STATE_CLOSING);
			
			try
			{
				// check with the peer to see if it is ok to close the channel
				zero.close();
			}
			catch (BEEPError e)
			{
				changeState(core.SessionState.SESSION_STATE_ACTIVE);
				throw e;
			}
			catch (BEEPException e)
			{
				//UPGRADE_TODO: The equivalent in .NET for method 'java.lang.Throwable.getMessage' may return a different value. 'ms-help://MS.VSCC.2003/commoner/redir/redirect.htm?keyword="jlca1043"'
				terminate(e.Message);
				log.error("Error sending close", e);
				throw e;
			}
			
			this.disableIO();
			// @todo close the socket
			
			channels.Clear();
			zero = null;
			
			this.changeState(core.SessionState.SESSION_STATE_CLOSED);
			fireSessionTerminated();
		}
		
		/// <summary>Get the local <code>SessionCredential</code> for this session.</summary>
		/// <returns>May return <code>null</code> if this session has not
		/// been authenticated</returns>
		public virtual SessionCredential getLocalCredential()
		{
			return localCredential;
		}
		
		/// <summary>Get our peer's <code>SessionCredential</code> for this session.</summary>
		/// <returns>May return <code>null</code> if this session has not
		/// been authenticated</returns>
		public virtual SessionCredential getPeerCredential()
		{
			return peerCredential;
		}
		
		/// <summary>Removes the listener from the list of listeners to be notified
		/// of future events. Note that the listener will be notified of
		/// events which have already happened and are in the process of
		/// being dispatched.</summary>
		/// <seealso cref="#addChannelListener"></seealso>
		public virtual void  removeChannelListener(IChannelListener l)
		{
			//UPGRADE_TODO: Method 'java.util.List.remove' was converted to 'System.Collections.IList.Remove' which has a different behavior. 'ms-help://MS.VSCC.2003/commoner/redir/redirect.htm?keyword="jlca1073_javautilListremove_javalangObject"'
			channelListeners.Remove(l);
		}
		
		/// <summary>Removes the listener from the list of listeners to be notified
		/// of future events. Note that the listener will be notified of
		/// events which have already happened and are in the process of
		/// being dispatched.</summary>
		/// <seealso cref="#addSessionListener"></seealso>
		public virtual void  removeSessionListener(ISessionListener l)
		{
			sessionListeners.Remove(l);
		}
		
		public virtual IChannel startChannel(string profile)
		{
			return startChannel(profile, (IRequestHandler) null);
		}
		
#if MESSAGELISTENER
		public virtual Channel startChannel(string profile, MessageListener listener)
		{
			StartChannelProfile p = new StartChannelProfile(profile);
			ArrayList l = new ArrayList();
			
			l.Add(p);
			
			return startChannelRequest(l, listener, false);
		}
#endif
		
		public virtual IChannel startChannel(string profile, IRequestHandler handler)
		{
			StartChannelProfile p = new StartChannelProfile(profile);
			ArrayList l = new ArrayList();
			
			l.Add(p);
			
			return startChannelRequest(l, handler, false);
		}
		
		public virtual IChannel startChannel(string profile, bool base64Encoding, string data)
		{
			StartChannelProfile p = new StartChannelProfile(profile, base64Encoding, data);

			return startChannel(p, null);
		}
		
#if MESSAGELISTENER
		public virtual Channel startChannel(string profile, bool base64Encoding, string data, MessageListener listener)
		{
			StartChannelProfile p = new StartChannelProfile(profile, base64Encoding, data);
			ArrayList l = new ArrayList();
			
			l.Add(p);
			
			return startChannelRequest(l, listener, false);
		}
#endif

		public virtual IChannel startChannel(StartChannelProfile profile, IRequestHandler handler)
		{
			ArrayList l = new ArrayList();
			
			l.Add(profile);
			
			return startChannelRequest(l, handler, false);
		}
		
#if COLLECTIONSUPPORT
		//UPGRADE_ISSUE: Class hierarchy differences between ''java.util.Collection'' and ''SupportClass.CollectionSupport'' may cause compilation errors. 'ms-help://MS.VSCC.2003/commoner/redir/redirect.htm?keyword="jlca1186"'
		public virtual Channel startChannel(SupportClass.CollectionSupport profiles, MessageListener listener)
		{
			return startChannelRequest(profiles, listener, false);
		}
		
		//UPGRADE_ISSUE: Class hierarchy differences between ''java.util.Collection'' and ''SupportClass.CollectionSupport'' may cause compilation errors. 'ms-help://MS.VSCC.2003/commoner/redir/redirect.htm?keyword="jlca1186"'
		public virtual Channel startChannel(SupportClass.CollectionSupport profiles, RequestHandler handler)
		{
			return startChannelRequest(profiles, handler, false);
		}
#endif

#if MESSAGELISTENER
		internal virtual Channel startChannelRequest(System.Collections.ICollection profiles, MessageListener listener, bool tuning)
		{
			return startChannelRequest(profiles, (RequestHandler)(listener == null?null:new ChannelImpl.MessageListenerAdapter(listener)), tuning);
		}
#endif

		internal virtual IChannel startChannelRequest(System.Collections.ICollection profiles, IRequestHandler handler, bool tuning)
		{
			string channelNumber = NextFreeChannelNumber;
			
			// create the message in a buffer and send it
			System.Text.StringBuilder startBuffer = new System.Text.StringBuilder();
			
			startBuffer.Append("<start number='");
			startBuffer.Append(channelNumber);
			if ((object) serverName != null && !sentServerName)
			{
				startBuffer.Append("' serverName='");
				startBuffer.Append(serverName);
			}
			startBuffer.Append("'>");
			
			foreach(StartChannelProfile p in profiles)
			{
				// @todo maybe we should check these against peerSupportedProfiles
				startBuffer.Append("<profile uri='");
				startBuffer.Append(p.uri);
				startBuffer.Append("' ");
				
				if ((object) p.data == null)
				{
					startBuffer.Append(" />");
				}
				else
				{
					if (p.base64Encoding)
					{
						startBuffer.Append("encoding='base64' ");
					}
					
					startBuffer.Append("><![CDATA[");
					startBuffer.Append(p.data);
					startBuffer.Append("]]></profile>");
				}
			}
			
			startBuffer.Append("</start>");
			
			// @todo handle the data element
			// Create a channel
			ChannelImpl ch = new ChannelImpl(null, channelNumber, handler, false, this);
			
			// Make a message
			OutputDataStream ds = new ByteOutputDataStream(MimeHeaders.BEEP_XML_CONTENT_TYPE, System.Text.Encoding.ASCII.GetBytes(startBuffer.ToString()));
			
			if (tuning)
			{
				this.changeState(core.SessionState.SESSION_STATE_TUNING_PENDING);
				this.changeState(core.SessionState.SESSION_STATE_TUNING);
				this.zero.setState(core.ChannelState.STATE_TUNING);
			}
			
			// Tell Channel Zero to start us up
			StartReplyListener reply = new StartReplyListener(this, ch);
			lock (reply)
			{
				this.zero.sendMSG(ds, reply);
				try
				{
					System.Threading.Monitor.Wait(reply);
				}
				catch (System.Threading.ThreadInterruptedException e)
				{
					log.error("Interrupted waiting for reply", e);
					throw new BEEPException("Interrupted waiting for reply");
				}
			}
			
			// check the channel state and return the appropriate exception
			if (reply.isError())
			{
				throw reply.getError();
			}
			
			if (ch.getState() != core.ChannelState.STATE_ACTIVE)
			{
				throw new BEEPException("Error channel state (" + ch.getState() + ")");
			}
			
			if (tuning)
			{
				ch.setState(core.ChannelState.STATE_TUNING);
			}
			
			if ((object) serverName != null)
			{
				sentServerName = true;
			}
			
			fireChannelStarted(ch);
			return ch;
		}
		
		/// <summary>This method is used to terminate the session when there is an
		/// non-recoverable error in the BEEP protocol (framing error, etc.).</summary>
		/// <param name="reason"></param>
		public virtual void  terminate(string reason)
		{
			log.error(reason);
			
			try
			{
				this.changeState(core.SessionState.SESSION_STATE_ABORTED);
			}
			catch (BEEPException)
			{				
				// Ignore this since we are terminating anyway.
			}
			
			this.disableIO();
			channels.Clear();
			
			zero = null;
			
			fireSessionTerminated();
		}
		
		/* (non-Javadoc)
		* @see java.lang.Object#toString()
		*/
		public override string ToString()
		{
			return base.ToString() + " (" + (Initiator?"I ":"L ") + StateString + ")";
		}
		
		internal virtual void changeState(core.SessionState newState)
		{
			lock (this)
			{
				try
				{
					ops[(int)state].changeState(this, newState);
				}
				catch (BEEPException e)
				{
					Console.Error.WriteLine(e.ToString());
					throw e;
				}
				
				if (log.isDebugEnabled())
				{
					log.debug("State changed to " + newState);
				}
			}
		}
		
		/// <summary>This method is intended for use by tranport specific Sessions to create
		/// a new <code>Frame</code> object representing a BEEP MSG, RPY, ERR,
		/// or NUL frames.</summary>
		/// <returns>a <code>Frame</code> for the specified values</returns>
		/// <exception cref="BEEPException" />
		protected internal virtual Frame createFrame(byte[] header, int headerLength)
		{
			Frame f = Frame.parseHeader(this, header, headerLength);
			
			// The window size and frame size have nothing in common.
			if (f.Size > ((ChannelImpl) f.Channel).AvailableWindow)
			{
				throw new BEEPException("Payload size is greater than channel " + "window size");
			}
			
			return f;
		}
		
		/// <summary>Method disableIO</summary>
		protected internal abstract void  disableIO();
		
		/// <summary>Method enableIO</summary>
		protected internal abstract void  enableIO();
		
		/// <summary>Returns the channel's available window size.</summary>
		protected internal virtual int getChannelAvailableWindow(int channel)
		{
			ChannelImpl ch = (ChannelImpl) channels[System.Convert.ToString(channel)];
			
			if (ch == null)
			{
				throw new BEEPException("Session call on nonexistent channel.");
			}
			
			return ch.AvailableWindow;
		}
		
		/// <summary>Get the channel number as a String</summary>
		/// <param name="channel"></param>
		protected internal virtual string getChannelNumberAsString(IChannel channel)
		{
			return ((ChannelImpl) channel).NumberAsString;
		}
		
		/// <summary>Method postFrame</summary>
		/// <param name="f"></param>
		/// <exception cref="BEEPException" />
		protected internal virtual bool postFrame(Frame f)
		{
			try
			{
				return ops[(int)state].postFrame(this, f);
			}
			catch (BEEPException e)
			{
				this.terminate(e.Message);
				
				return false;
			}
			catch (System.Exception e)
			{
				log.error("Error posting frame", e);
				this.terminate("Uncaught exception, terminating session");
				
				return false;
			}
		}
		
		/// <summary>This method is used by a tuning profile to reset the session after the
		/// tuning is complete.</summary>
		/// <returns>A new <code>Session</code> with the tuning complete.</returns>
		protected internal abstract ISession reset(SessionCredential localCred, SessionCredential peerCred, SessionTuningProperties tuning, ProfileRegistry registry, object argument);
		
		/// <summary>Implement this method to send frames and on the sub-classed transport.</summary>
		/// <param name="f">BEEP frame to send.
		/// </param>
		/// <exception cref="BEEPException" />
		protected internal abstract void  sendFrame(Frame f);
		
		/// <summary>Method setLocalCredential</summary>
		/// <param name="cred"></param>
		protected internal virtual void  setLocalCredential(SessionCredential cred)
		{
			localCredential = cred;
		}
		
		/// <summary>Method setPeerCredential</summary>
		/// <param name="cred"></param>
		protected internal virtual void  setPeerCredential(SessionCredential cred)
		{
			peerCredential = cred;
		}
		
		/// <summary>sets the tuning properties for this session</summary>
		/// <param name="tuning"></param>
		/// <seealso cref="SessionTuningProperties"></seealso>
		protected internal virtual void  setTuningProperties(SessionTuningProperties tuning)
		{
			tuningProperties = tuning;
		}
		
		public virtual SessionTuningProperties getTuningProperties()
		{
			return tuningProperties;
		}
		
		/// <summary>This method is designed to allow for flow control across the multiplexed
		/// connection we have. <p> The idea is to throttle data being sent over
		/// this session to be manageable per Channel, so that a given Channel
		/// doesn't take up all the bandwidth. <p>
		/// This method restricts the bufferSize, per the beep spec, to be at most
		/// two-thirds of the socket's receiveBufferSize.  If a size is requested
		/// beyond that, an exception is thrown.</summary>
		/// <param name="channel"></param>
		/// <param name="currentSeq"></param>
		/// <param name="currentAvail"></param>
		/// <returns>true if the Receive Buffer Size was updated</returns>
		/// <exception cref="">throws BEEPException if a specified buffer size is larger
		/// than what's available on the Socket.
		/// </exception><exception cref="BEEPException" />
		protected internal abstract bool updateMyReceiveBufferSize(IChannel channel, long currentSeq, int currentAvail);
		
		// @todo update the java-doc to correctly identify the params
		
		/// <summary>Method updatePeerReceiveBufferSize</summary>
		/// <param name="channelNum"></param>
		/// <param name="lastSeq"></param>
		/// <param name="">size
		/// </param>
		/// <exception cref="BEEPException" />
		protected internal virtual void  updatePeerReceiveBufferSize(int channelNum, long lastSeq, int size)
		{
			ChannelImpl channel = getValidChannel(channelNum);
			
			channel.updatePeerReceiveBufferSize(lastSeq, size);
		}
		
		/// <summary>The Initiator Oriented close channel call...but this one is not an
		/// external call, it's invoked from Channel.close();</summary>
		/// <param name="channel"></param>
		/// <param name="code"></param>
		/// <param name="xmlLang"></param>
		/// <exception cref="BEEPException" />
		internal virtual void  closeChannel(ChannelImpl channel, BEEPStatusCode code, string xmlLang)
		{
			
			// Construct Message
			System.Text.StringBuilder closeBuffer = new System.Text.StringBuilder();
			
			closeBuffer.Append("<close number='");
			closeBuffer.Append(channel.NumberAsString);
			closeBuffer.Append("' code='");
			closeBuffer.Append(code);
			
			if ((object) xmlLang != null)
			{
				closeBuffer.Append("' xml:lang='");
				closeBuffer.Append(xmlLang);
			}
			
			closeBuffer.Append("' />");
			
			// Lock necessary because we have to know the msgNo
			// before we send the message, in order to be able
			// to associate the reply with this start request
			CloseReplyListener reply = new CloseReplyListener(this, channel);
			lock (reply)
			{
				OutputDataStream ds = new ByteOutputDataStream(MimeHeaders.BEEP_XML_CONTENT_TYPE, System.Text.Encoding.ASCII.GetBytes(closeBuffer.ToString()));
				
				this.zero.sendMSG(ds, reply);
				try
				{
					System.Threading.Monitor.Wait(reply);
				}
				catch (System.Threading.ThreadInterruptedException e)
				{
					log.error("Error waiting for reply", e);
					throw new BEEPException("Interrupted waiting for reply");
				}
			}
			
			// check the channel state and return the appropriate exception
			if (reply.isError())
			{
				throw reply.getError();
			}
			
			if (channel.getState() != core.ChannelState.STATE_CLOSED)
			{
				throw new BEEPException("Error channel state (" + channel.getState() + ")");
			}
			
			fireChannelClosed(channel);
		}
		
		internal virtual ChannelImpl getValidChannel(int number)
		{
			ChannelImpl ch = (ChannelImpl) channels[System.Convert.ToString(number)];
			
			if (ch == null)
			{
				throw new BEEPException("Session call on nonexistent channel.");
			}
			
			return ch;
		}
		
		internal virtual void  sendProfile(string uri, string datum, ChannelImpl ch)
		{
			
			// Send the profile
			System.Text.StringBuilder sb = new System.Text.StringBuilder();
			
			sb.Append("<profile uri='");
			sb.Append(uri);
			
			if ((object) datum != null)
			{
				sb.Append("'><![CDATA[");
				sb.Append(datum);
				sb.Append("]]></profile>");
			}
			else
			{
				sb.Append("' />");
			}
			
			OutputDataStream ds = new ByteOutputDataStream(MimeHeaders.BEEP_XML_CONTENT_TYPE, System.Text.Encoding.ASCII.GetBytes(sb.ToString()));
			
			// Store the Channel
			channels[ch.NumberAsString] = ch;
			((IMessageMSG) zero.AppData).sendRPY(ds);
		}
		
		private void  fireChannelClosed(IChannel c)
		{
			if (this.channelListeners.Count == 0)
				return;

			ChannelEventArgs e = new ChannelEventArgs(c);
			foreach(IChannelListener l in this.channelListeners) 
			{
                l.channelClosed(e);
			}
		}
		
		internal virtual void  fireChannelStarted(IChannel c)
		{
			if (this.channelListeners.Count == 0)
				return;

			ChannelEventArgs e = new ChannelEventArgs(c);
			foreach(IChannelListener l in this.channelListeners) 
			{
				l.channelStarted(e);
			}
		}
		
		private void  fireGreetingReceived()
		{
			if (this.sessionListeners.Count == 0)
				return;

			SessionEventArgs e = new SessionEventArgs(this);
			foreach(ISessionListener l in this.sessionListeners) 
			{
				l.greetingReceived(e);
			}
		}
		
		private void  fireSessionClosed()
		{
			if (this.sessionListeners.Count == 0)
				return;

			SessionEventArgs e = new SessionEventArgs(this);
			foreach(ISessionListener l in this.sessionListeners) 
			{
				l.sessionClosed(e);
			}
		}
		
		private void  fireSessionTerminated()
		{
			if (this.sessionListeners.Count == 0)
				return;

			SessionEventArgs e = new SessionEventArgs(this);
			foreach(ISessionListener l in this.sessionListeners) 
			{
				l.sessionClosed(e);
			}
		}
		
		protected internal virtual void  fireSessionReset(ISession newSession)
		{
			if (this.sessionListeners.Count == 0)
				return;

			SessionResetEventArgs e = new SessionResetEventArgs(this, newSession);
			foreach(ISessionListener l in this.sessionListeners) 
			{
				l.sessionReset(e);
			}
		}
		
		/// <summary>This method is called when Channel Zero receives - from our
		/// session peer - a request to close a channel.</summary>
		/// <param name="channelNumber"></param>
		/// <param name="code"></param>
		/// <param name="xmlLang"></param>
		/// <param name="data"></param>
		/// <exception cref="BEEPException" />
		private void  receiveCloseChannel(string channelNumber, string code, string xmlLang, string data)
		{
			
			// @todo fix close channel
			if (channelNumber.Equals(CHANNEL_ZERO))
			{
				receiveCloseChannelZero();
				
				return ;
			}
			
			enableIO();
			
			ChannelImpl channel = (ChannelImpl) channels[channelNumber];
			
			if (channel == null)
			{
				throw new BEEPError(BEEPStatusCode.PARAMETER_INVALID, "Close requested for nonexistent channel");
			}
			
			try
			{
				IStartChannelListener scl = profileRegistry.getStartChannelListener(this.tuningProperties, channel.getProfile());
				
				scl.CloseChannel(channel);
				channel.setState(core.ChannelState.STATE_CLOSING);
			}
			catch (BEEPError x)
			{
				channel.setState(core.ChannelState.STATE_CLOSING);
				
				throw x;
			}
			
			// Send an ok
			OutputDataStream sds = new ByteOutputDataStream(MimeHeaders.BEEP_XML_CONTENT_TYPE, OK_ELEMENT);
			
			try
			{
				((IMessageMSG) zero.AppData).sendRPY(sds);
			}
			catch (BEEPException)
			{
				terminate("Error sending RPY for <close>");
				
				return ;
			}

			// We need to close the server socket at some point !
			this.disableIO();
			
			// We're past the CCL approval
			channel.setState(core.ChannelState.STATE_CLOSED);
			channels.Remove(channel.NumberAsString);
			fireChannelClosed(channel);
		}
		
		private void  receiveCloseChannelZero()
		{
			
			// closing the session
			// @todo fireEvent(SESSION_STATE_CLOSING);
			
			if (log.isDebugEnabled())
			{
				log.debug("Closing Session with " + channels.Count + " channels");
			}
			
			try
			{
				changeState(core.SessionState.SESSION_STATE_CLOSE_PENDING);
				changeState(core.SessionState.SESSION_STATE_CLOSING);
			}
			catch (BEEPException)
			{
				terminate("Error changing Session state to closing.");
				return ;
			}
			
			object[] keys = new object[channels.Keys.Count];
			channels.Keys.CopyTo(keys,0);

			foreach(object key in keys)
			{
				ChannelImpl ch = (ChannelImpl) channels[key];
				
				// if this channel is not zero, call the channel's scl
				if (ch.Number == 0)
				{
					continue;
				}
				
				IStartChannelListener scl = profileRegistry.getStartChannelListener(this.tuningProperties, ch.getProfile());
				
				// check locally first to see if it is ok to close the channel
				try
				{
					scl.CloseChannel(ch);

					channels.Remove(key);
				}
				catch (CloseChannelException e)
				{
					try
					{
						changeState(core.SessionState.SESSION_STATE_ACTIVE);
						
						enableIO();
						throw e;
					}
					catch (BEEPException)
					{
						terminate("Error changing Session state from closing " + "to active");
						
						return ;
					}
				}
				fireChannelClosed(ch);
			}
			
			OutputDataStream sds = new ByteOutputDataStream(MimeHeaders.BEEP_XML_CONTENT_TYPE, OK_ELEMENT);
			
			try
			{
				((IMessageMSG) zero.AppData).sendRPY(sds);
			}
			catch (BEEPException)
			{
				terminate("Error sending RPY for <close> for channel 0");
				
				return ;
			}
			
			try
			{
				this.changeState(core.SessionState.SESSION_STATE_CLOSED);
			}
			catch (BEEPException e)
			{
				log.error("Error changing state", e);
			}
			
			fireSessionClosed();
		}
		
		/// <summary>Listener oriented Start Channel call, a call here means that
		/// we've received a start channel request over the wire.</summary>
		//UPGRADE_ISSUE: Class hierarchy differences between ''java.util.Collection'' and ''SupportClass.CollectionSupport'' may cause compilation errors. 'ms-help://MS.VSCC.2003/commoner/redir/redirect.htm?keyword="jlca1186"'
		private void  processStartChannel(string channelNumber, System.Collections.ICollection profiles)
		{
			IStartChannelListener scl;
			ChannelImpl ch = null;
			System.Collections.IEnumerator i = profiles.GetEnumerator();
			
			//UPGRADE_TODO: Method 'java.util.Iterator.hasNext' was converted to 'System.Collections.IEnumerator.MoveNext' which has a different behavior. 'ms-help://MS.VSCC.2003/commoner/redir/redirect.htm?keyword="jlca1073_javautilIteratorhasNext"'
			while (i.MoveNext())
			{
				//UPGRADE_TODO: Method 'java.util.Iterator.next' was converted to 'System.Collections.IEnumerator.Current' which has a different behavior. 'ms-help://MS.VSCC.2003/commoner/redir/redirect.htm?keyword="jlca1073_javautilIteratornext"'
				StartChannelProfile p = (StartChannelProfile) i.Current;
				
				scl = profileRegistry.getStartChannelListener(this.tuningProperties, p.uri);
				
				if (scl == null)
				{
					continue;
				}
				
				ch = new ChannelImpl(p.uri, channelNumber, this);
				
				try
				{
					string encoding = p.base64Encoding?"base64":"none";
					
					scl.StartChannel(ch, encoding, p.data);
				}
				catch (StartChannelException e)
				{
					this.enableIO();
					
					try
					{
						((IMessageMSG) zero.AppData).sendERR(e);
					}
					catch (BEEPException)
					{
						terminate("Error sending ERR response to start channel");
					}
					
					return ;
				}
				
				if ((object) p.data != null && (object) ch.StartData == null)
				{
					byte[] data;
					if (p.base64Encoding)
					{
						try
						{
							data = Convert.FromBase64String(p.data);
						}
						catch (System.FormatException)
						{
							ch.abort();
							this.enableIO();
							throw new BEEPError(BEEPStatusCode.REQUESTED_ACTION_ABORTED, "Error parsing piggybacked data.");
						}
					}
					else
					{
						data = System.Text.Encoding.UTF8.GetBytes(p.data);
					}
					
					PiggybackedMSG msg = new PiggybackedMSG(ch, data, p.base64Encoding);
					
					ch.setState(core.ChannelState.STATE_STARTING);
					
					try
					{
						ch.addPiggybackedMSG(msg);
					}
					catch (BEEPException e)
					{
						//UPGRADE_TODO: The equivalent in .NET for method 'java.lang.Throwable.getMessage' may return a different value. 'ms-help://MS.VSCC.2003/commoner/redir/redirect.htm?keyword="jlca1043"'
						terminate("Error sending profile. " + e.Message);
						
						return ;
					}
				}
				else
				{
					try
					{
						sendProfile(p.uri, ch.StartData, ch);
						ch.setState(core.ChannelState.STATE_ACTIVE);
					}
					catch (BEEPException e)
					{
						//UPGRADE_TODO: The equivalent in .NET for method 'java.lang.Throwable.getMessage' may return a different value. 'ms-help://MS.VSCC.2003/commoner/redir/redirect.htm?keyword="jlca1043"'
						terminate("Error sending profile. " + e.Message);
						
						return ;
					}
					
					fireChannelStarted(ch);
					
					if ((object) p.data == null && ch.getState() != core.ChannelState.STATE_TUNING)
					{
						this.enableIO();
					}
				}
				
				return ;
			}
			
			this.enableIO();
			
			try
			{
				((IMessageMSG) zero.AppData).sendERR(BEEPStatusCode.REQUESTED_ACTION_NOT_TAKEN2, "all requested profiles are unsupported");
			}
			catch (System.Exception x)
			{
				//UPGRADE_TODO: The equivalent in .NET for method 'java.lang.Throwable.getMessage' may return a different value. 'ms-help://MS.VSCC.2003/commoner/redir/redirect.htm?keyword="jlca1043"'
				terminate("Error sending error. " + x.Message);
			}
		}
		
		//UPGRADE_TODO: Interface 'w3c.dom.Element' was not converted. 'ms-help://MS.VSCC.2003/commoner/redir/redirect.htm?keyword="jlca1095"'
		private XmlElement processMessage(IMessage message)
		{
			
			// check the message content type
			if (!message.DataStream.InputStream.ContentType.Equals(MimeHeaders.BEEP_XML_CONTENT_TYPE))
			{
				throw new BEEPException("Invalid content type for this message");
			}
			
			// parse the stream
			XmlDocument doc = new XmlDocument();
			
			try
			{
				doc.Load(message.DataStream.InputStream);
			}
			catch (System.Xml.XmlException se)
			{
				throw new BEEPException(ERR_MALFORMED_XML_MSG,se);
			}
			
			XmlElement topElement = doc.DocumentElement;
			
			if (topElement == null)
			{
				throw new BEEPException(ERR_MALFORMED_XML_MSG);
			}
			
			return topElement;
		}
		
		private void  sendGreeting()
		{
			log.debug("sendGreeting");
			
			// get the greeting from the session
			byte[] greeting = this.ProfileRegistry.getGreeting(this);
			ByteOutputDataStream f = new ByteOutputDataStream(MimeHeaders.BEEP_XML_CONTENT_TYPE, greeting);
			
			IMessageMSG m = new MessageMSGImpl(this.zero, 0, null);
			
			// send the greeting
			m.sendRPY(f);
		}
		
		//UPGRADE_NOTE: Field 'EnclosingInstance' was added to class 'ChannelZeroListener' to access its enclosing instance. 'ms-help://MS.VSCC.2003/commoner/redir/redirect.htm?keyword="jlca1019"'
#if MESSAGELISTENER
		private class ChannelZeroListener : MessageListener
#else
		private class ChannelZeroListener : IRequestHandler
#endif
		{
			public ChannelZeroListener(SessionImpl enclosingInstance)
			{
				InitBlock(enclosingInstance);
			}
			private void  InitBlock(SessionImpl enclosingInstance)
			{
				this.enclosingInstance = enclosingInstance;
			}

			private SessionImpl enclosingInstance;
			public SessionImpl Enclosing_Instance
			{
				get
				{
					return enclosingInstance;
				}
			}
			
#if MESSAGELISTENER
			public virtual void ReceiveMSG(Message message)
#else
			public virtual void ReceiveMSG(IMessageMSG message)
#endif
			{
				XmlElement topElement;
				
				try
				{
					topElement = Enclosing_Instance.processMessage(message);
				}
				catch (BEEPException)
				{
					throw new BEEPError(BEEPStatusCode.GENERAL_SYNTAX_ERROR, beepcore.beep.core.SessionImpl.ERR_MALFORMED_XML_MSG);
				}
				
				string elementName = topElement.Name;
				
				if ((object) elementName == null)
				{
					throw new BEEPError(BEEPStatusCode.PARAMETER_ERROR, beepcore.beep.core.SessionImpl.ERR_MALFORMED_XML_MSG);
				}
				
				// is this MSG a <start>
				if (elementName.Equals("start"))
				{
					Enclosing_Instance.log.debug("Received a start channel request");
					
					string channelNumber = topElement.GetAttribute("number");
					
					if ((object) channelNumber == null)
					{
						throw new BEEPError(BEEPStatusCode.PARAMETER_ERROR, "Malformed <start>: no channel number");
					}
					
					// this attribute is implied
					string serverName = topElement.GetAttribute("serverName");
					XmlNodeList profiles = topElement.GetElementsByTagName("profile");
					
					if (profiles == null)
					{
						throw new BEEPError(BEEPStatusCode.PARAMETER_ERROR, "Malformed <start>: no profiles");
					}
					
					ArrayList profileList = new ArrayList();
					
					for (int i = 0; i < profiles.Count; i++)
					{
						XmlElement profile = (XmlElement) profiles.Item(i);
						string uri = profile.GetAttribute("uri");
						
						if ((object) uri == null)
						{
							throw new BEEPError(BEEPStatusCode.PARAMETER_ERROR, "no profiles in start");
						}
						
						string encoding = profile.GetAttribute("encoding");
						bool b64;
						
						if (((object) encoding == null) || encoding.Equals(""))
						{
							b64 = false;
						}
						else if (encoding.ToUpper().Equals("base64".ToUpper()))
						{
							b64 = true;
						}
						else if (encoding.ToUpper().Equals("none".ToUpper()))
						{
							b64 = false;
						}
						else
						{
							throw new BEEPError(BEEPStatusCode.PARAMETER_ERROR, "unkown encoding in start");
						}
						
						string data = null;

						XmlNode dataNode = profile.FirstChild;
						
						if (dataNode != null)
						{
							data = dataNode.InnerText;
							
							if (data.Length > beepcore.beep.core.SessionImpl.MAX_PCDATA_SIZE)
							{
								throw new BEEPError(BEEPStatusCode.PARAMETER_ERROR, "Element's PCDATA exceeds " + "the maximum size");
							}
						}
						
						profileList.Add(new StartChannelProfile(uri, b64, data));
					}
					
					Enclosing_Instance.zero.AppData = message;
					Enclosing_Instance.processStartChannel(channelNumber, profileList);
				}
				// is this MSG a <close>
				else if (elementName.Equals("close"))
				{
					Enclosing_Instance.log.debug("Received a channel close request");
					
					try
					{
						string channelNumber = topElement.GetAttribute("number");
						
						if ((object) channelNumber == null)
						{
							throw new BEEPError(BEEPStatusCode.PARAMETER_ERROR, "Malformed <close>: no channel number");
						}
						
						string code = topElement.GetAttribute("code");
						
						if ((object) code == null)
						{
							throw new BEEPError(BEEPStatusCode.PARAMETER_ERROR, "Malformed <close>: no code attribute");
						}
						
						// this attribute is implied
						string xmlLang = topElement.GetAttribute("xml:lang");
						string data = null;

						XmlNode dataNode = topElement.FirstChild;
						
						if (dataNode != null)
						{
							data = dataNode.InnerText;
							
							if (data.Length > beepcore.beep.core.SessionImpl.MAX_PCDATA_SIZE)
							{
								throw new BEEPError(BEEPStatusCode.PARAMETER_ERROR, "Element's PCDATA exceeds " + "the maximum size");
							}
						}
						Enclosing_Instance.zero.AppData = message;
						Enclosing_Instance.receiveCloseChannel(channelNumber, code, xmlLang, data);
					}
					catch (BEEPError e)
					{
						Enclosing_Instance.enableIO();
						throw e;
					}
				}
				else
				{
					throw new BEEPError(BEEPStatusCode.PARAMETER_ERROR, beepcore.beep.core.SessionImpl.ERR_UNKNOWN_OPERATION_ELEMENT_MSG);
				}
			}
		}
		
		//UPGRADE_NOTE: Field 'EnclosingInstance' was added to class 'GreetingListener' to access its enclosing instance. 'ms-help://MS.VSCC.2003/commoner/redir/redirect.htm?keyword="jlca1019"'
		private class GreetingListener : ReplyListener
		{
			public GreetingListener(SessionImpl enclosingInstance)
			{
				InitBlock(enclosingInstance);
			}
			private void  InitBlock(SessionImpl enclosingInstance)
			{
				this.enclosingInstance = enclosingInstance;
			}
			private SessionImpl enclosingInstance;
			public SessionImpl Enclosing_Instance
			{
				get
				{
					return enclosingInstance;
				}
				
			}
			
			public virtual void  receiveRPY(IMessage message)
			{
				try
				{
					//UPGRADE_TODO: Interface 'w3c.dom.Element' was not converted. 'ms-help://MS.VSCC.2003/commoner/redir/redirect.htm?keyword="jlca1095"'
					XmlElement topElement = Enclosing_Instance.processMessage(message);
					
					// is this RPY a <greeting>
					//UPGRADE_TODO: Method 'w3c.dom.Element.getTagName' was not converted. 'ms-help://MS.VSCC.2003/commoner/redir/redirect.htm?keyword="jlca1095"'
					string elementName = topElement.Name;
					
					if ((object) elementName == null)
					{
						throw new BEEPException(beepcore.beep.core.SessionImpl.ERR_MALFORMED_XML_MSG);
					}
					else if (!elementName.Equals("greeting"))
					{
						throw new BEEPException(beepcore.beep.core.SessionImpl.ERR_UNKNOWN_OPERATION_ELEMENT_MSG);
					}
					
					Enclosing_Instance.log.debug("Received a greeting");
					
					// this attribute is implied
					string features = topElement.GetAttribute("features");
					
					// This attribute has a default value
					string localize = topElement.GetAttribute("localize");
					
					if ((object) localize == null)
					{
						localize = core.StringConstants.LOCALIZE_DEFAULT;
					}
					
					// Read the profiles - note, the greeting is valid
					// with 0 profiles
					XmlNodeList profiles = topElement.GetElementsByTagName("profile");
					
					if (profiles.Count > 0)
					{
						ArrayList profileList = new ArrayList();
						
						for (int i = 0; i < profiles.Count; i++)
						{
							XmlElement profile = (XmlElement) profiles.Item(i);
							string uri = profile.GetAttribute("uri");
							
							if ((object) uri == null)
							{
								throw new BEEPException("Malformed profile");
							}
							
							string encoding = profile.GetAttribute("encoding");
							
							// encoding is not allowed in greetings
							if ((object) encoding != null)
							{
								// @todo check this
								// terminate("Invalid attribute 'encoding' in greeting.");
								// return;
							}
							
							profileList.Insert(i, uri);
						}
						
						Enclosing_Instance.peerSupportedProfiles = ArrayList.ReadOnly(profileList);
					}
					
					Enclosing_Instance.changeState(core.SessionState.SESSION_STATE_ACTIVE);
					
					lock (this)
					{
						System.Threading.Monitor.PulseAll(this);
					}
				}
				catch (BEEPException e)
				{
					enclosingInstance.terminate("Problem with RPY: " + e.Message);
				}
			}
			
			public virtual void  receiveERR(IMessage message)
			{
				enclosingInstance.terminate("Received an unexpected ERR");
			}
			
			public virtual void  receiveANS(IMessage message)
			{
				enclosingInstance.terminate("Received an unexpected ANS");
			}
			
			public virtual void  receiveNUL(IMessage message)
			{
				enclosingInstance.terminate("Received an unexpected NUL");
			}
		}
		
		//UPGRADE_NOTE: Field 'EnclosingInstance' was added to class 'StartReplyListener' to access its enclosing instance. 'ms-help://MS.VSCC.2003/commoner/redir/redirect.htm?keyword="jlca1019"'
		private class StartReplyListener : ReplyListener
		{
			private void  InitBlock(SessionImpl enclosingInstance)
			{
				this.enclosingInstance = enclosingInstance;
			}
			private SessionImpl enclosingInstance;
			public SessionImpl Enclosing_Instance
			{
				get
				{
					return enclosingInstance;
				}
				
			}
			
			internal ChannelImpl channel;
			internal BEEPError error;
			
			internal StartReplyListener(SessionImpl enclosingInstance, ChannelImpl channel)
			{
				InitBlock(enclosingInstance);
				this.channel = channel;
				this.error = null;
			}
			
			internal virtual bool isError()
			{
				return this.error != null;
			}
			
			internal virtual BEEPError getError()
			{
				return this.error;
			}
			
			public virtual void  receiveRPY(IMessage message)
			{
				try
				{
					XmlElement topElement = Enclosing_Instance.processMessage(message);
					
					// is this RPY a <greeting>
					string elementName = topElement.Name;
					
					if ((object) elementName == null)
					{
						throw new BEEPException(beepcore.beep.core.SessionImpl.ERR_MALFORMED_XML_MSG);
						
						// is this RPY a <profile>
					}
					else if (elementName.Equals("profile"))
					{
						try
						{
							string uri = topElement.GetAttribute("uri");
							
							if ((object) uri == null)
							{
								throw new BEEPException("Malformed profile");
							}
							
							string encoding = topElement.GetAttribute("encoding");
							
							if ((object) encoding == null)
							{
								encoding = core.StringConstants.ENCODING_NONE;
							}
							
							// see if there is data and then turn it into a message
							XmlNode dataNode = topElement.FirstChild;
							string data = null;
							
							if (dataNode != null)
							{
								data = dataNode.InnerText;
								
								if (data.Length > beepcore.beep.core.SessionImpl.MAX_PCDATA_SIZE)
								{
									throw new BEEPException("Element's PCDATA " + "exceeds the " + "maximum size");
								}
							}
							
							channel.Encoding = encoding;
							channel.setProfile(uri);
							channel.StartData = data;
							
							// set the state
							channel.setState(core.ChannelState.STATE_ACTIVE);
							Enclosing_Instance.channels[channel.NumberAsString] = channel;
							
							/// <summary>@todo something with data</summary>
							
							// release the block waiting for the channel
							// to start or close
							lock (this)
							{
								System.Threading.Monitor.Pulse(this);
							}
						}
						catch (System.Exception x)
						{
							throw new BEEPException(x);
						}
					}
					else
					{
						throw new BEEPException(beepcore.beep.core.SessionImpl.ERR_UNKNOWN_OPERATION_ELEMENT_MSG);
					}
				}
				catch (BEEPException e)
				{
					enclosingInstance.terminate("Problem with RPY: " + e.Message);
				}
			}
			
			public virtual void  receiveERR(IMessage message)
			{
				BEEPError err;
				
				try
				{
					err = BEEPError.convertMessageERRToException(message);
				}
				catch (BEEPException e)
				{
					enclosingInstance.terminate(e.Message);
					
					return ;
				}
				
				Enclosing_Instance.log.error("Received an error in response to a start. code=" + err.Code + " diagnostic=" + err.Diagnostic);
				
				this.error = err;
				
				channel.setState(core.ChannelState.STATE_CLOSED);
				Enclosing_Instance.channels.Remove(channel.NumberAsString);
				
				// release the block waiting for the channel to start or close
				lock (this)
				{
					System.Threading.Monitor.Pulse(this);
				}
			}
			
			public virtual void  receiveANS(IMessage message)
			{
				enclosingInstance.terminate("Received an unexpected ANS");
			}
			
			public virtual void  receiveNUL(IMessage message)
			{
				enclosingInstance.terminate("Received an unexpected NUL");
			}
		}
		
		//UPGRADE_NOTE: Field 'EnclosingInstance' was added to class 'CloseReplyListener' to access its enclosing instance. 'ms-help://MS.VSCC.2003/commoner/redir/redirect.htm?keyword="jlca1019"'
		private class CloseReplyListener : ReplyListener
		{
			private void  InitBlock(SessionImpl enclosingInstance)
			{
				this.enclosingInstance = enclosingInstance;
			}
			private SessionImpl enclosingInstance;
			public SessionImpl Enclosing_Instance
			{
				get
				{
					return enclosingInstance;
				}
				
			}
			
			internal ChannelImpl channel;
			internal BEEPError error;
			
			internal CloseReplyListener(SessionImpl enclosingInstance, ChannelImpl channel)
			{
				InitBlock(enclosingInstance);
				this.channel = channel;
				this.error = null;
			}
			
			internal virtual bool isError()
			{
				return this.error != null;
			}
			
			internal virtual BEEPError getError()
			{
				return this.error;
			}
			
			public virtual void  receiveRPY(IMessage message)
			{
				try
				{
					//UPGRADE_TODO: Interface 'w3c.dom.Element' was not converted. 'ms-help://MS.VSCC.2003/commoner/redir/redirect.htm?keyword="jlca1095"'
					XmlElement topElement = Enclosing_Instance.processMessage(message);
					//UPGRADE_TODO: Method 'w3c.dom.Element.getTagName' was not converted. 'ms-help://MS.VSCC.2003/commoner/redir/redirect.htm?keyword="jlca1095"'
					string elementName = topElement.Name;
					
					if ((object) elementName == null)
					{
						throw new BEEPException(beepcore.beep.core.SessionImpl.ERR_MALFORMED_XML_MSG);
						
						// is this RPY an <ok> (the positive response to a
						// channel close)
					}
					else if (elementName.Equals("ok"))
					{
						Enclosing_Instance.log.debug("Received an OK for channel close");
						
						// @todo we should fire an event instead.
						// set the state
						channel.setState(core.ChannelState.STATE_CLOSING);
						Enclosing_Instance.channels.Remove(channel.NumberAsString);
						channel.setState(core.ChannelState.STATE_CLOSED);
						
						// release the block waiting for the channel to
						// start or close
						lock (this)
						{
							System.Threading.Monitor.Pulse(this);
						}
					}
					else
					{
						throw new BEEPException(beepcore.beep.core.SessionImpl.ERR_UNKNOWN_OPERATION_ELEMENT_MSG);
					}
				}
				catch (BEEPException e)
				{
					//UPGRADE_TODO: The equivalent in .NET for method 'java.lang.Throwable.getMessage' may return a different value. 'ms-help://MS.VSCC.2003/commoner/redir/redirect.htm?keyword="jlca1043"'
					enclosingInstance.terminate("Problem with RPY: " + e.Message);
				}
			}
			
			public virtual void  receiveERR(IMessage message)
			{
				BEEPError err;
				
				try
				{
					err = BEEPError.convertMessageERRToException(message);
				}
				catch (BEEPException e)
				{
					//UPGRADE_TODO: The equivalent in .NET for method 'java.lang.Throwable.getMessage' may return a different value. 'ms-help://MS.VSCC.2003/commoner/redir/redirect.htm?keyword="jlca1043"'
					enclosingInstance.terminate(e.Message);
					
					return ;
				}
				
				Enclosing_Instance.log.debug("Received an error in response to a close. code=" + err.Code + " diagnostic=" + err.Diagnostic);
				
				this.error = err;
				
				// set the state
				channel.setState(core.ChannelState.STATE_ACTIVE);
				Enclosing_Instance.channels.Remove(channel.NumberAsString);
				
				// release the block waiting for the channel to start or close
				lock (this)
				{
					System.Threading.Monitor.Pulse(this);
				}
			}
			
			public virtual void  receiveANS(IMessage message)
			{
				enclosingInstance.terminate("Received an unexpected ANS");
			}
			
			public virtual void  receiveNUL(IMessage message)
			{
				enclosingInstance.terminate("Received an unexpected NUL");
			}
		}
		
		internal interface SessionOperations
		{
			void  changeState(SessionImpl s, core.SessionState newState);
			bool postFrame(SessionImpl s, Frame f);
		}
		
		internal class INITIALIZED_SessionOperations : SessionOperations
		{
			public INITIALIZED_SessionOperations()
			{
				InitBlock();
			}
			private void  InitBlock()
			{
				log = LogFactory.getLog(this.GetType());
			}
			public virtual void  changeState(SessionImpl s, core.SessionState newState)
			{
				if (!((newState == core.SessionState.SESSION_STATE_GREETING_SENT) || (newState == core.SessionState.SESSION_STATE_ABORTED)))
				{
					throw new BEEPException("Illegal session state transition");
				}
				
				s.state = newState;
			}
			
			public virtual bool postFrame(SessionImpl s, Frame f)
			{
				// If we're in a PRE-GREETING state
				// only handle one frame at a time...
				// to avoid processing post-greeting
				// frames before the greeting has been
				// fully handled.
				lock (s)
				{
					return ((ChannelImpl) f.Channel).postFrame(f);
				}
			}
			
			//UPGRADE_NOTE: The initialization of  'log' was moved to method 'InitBlock'. 'ms-help://MS.VSCC.2003/commoner/redir/redirect.htm?keyword="jlca1005"'
			private Log log;
		}
		
		internal class GREETING_SENT_SessionOperations : SessionOperations
		{
			public GREETING_SENT_SessionOperations()
			{
				InitBlock();
			}
			private void  InitBlock()
			{
				log = LogFactory.getLog(this.GetType());
			}
			public virtual void  changeState(SessionImpl s, core.SessionState newState)
			{
				if (!((newState == core.SessionState.SESSION_STATE_ACTIVE) || (newState == core.SessionState.SESSION_STATE_ABORTED)))
				{
					throw new BEEPException("Illegal session state transition");
				}
				
				s.state = newState;
			}
			
			public virtual bool postFrame(SessionImpl s, Frame f)
			{
				// If we're in a PRE-GREETING state
				// only handle one frame at a time...
				// to avoid processing post-greeting
				// frames before the greeting has been
				// fully handled.
				lock (s)
				{
					return ((ChannelImpl) f.Channel).postFrame(f);
				}
			}
			
			//UPGRADE_NOTE: The initialization of  'log' was moved to method 'InitBlock'. 'ms-help://MS.VSCC.2003/commoner/redir/redirect.htm?keyword="jlca1005"'
			private Log log;
		}
		
		internal class ACTIVE_SessionOperations : SessionOperations
		{
			public ACTIVE_SessionOperations()
			{
				InitBlock();
			}
			private void  InitBlock()
			{
				log = LogFactory.getLog(this.GetType());
			}
			public virtual void  changeState(SessionImpl s, core.SessionState newState)
			{
				if (!((newState == core.SessionState.SESSION_STATE_TUNING_PENDING) || (newState == core.SessionState.SESSION_STATE_CLOSE_PENDING) || (newState == core.SessionState.SESSION_STATE_ABORTED)))
				{
					throw new BEEPException("Illegal session state transition");
				}
				
				s.state = newState;
			}
			
			public virtual bool postFrame(SessionImpl s, Frame f)
			{
				return ((ChannelImpl) f.Channel).postFrame(f);
			}
			
			//UPGRADE_NOTE: The initialization of  'log' was moved to method 'InitBlock'. 'ms-help://MS.VSCC.2003/commoner/redir/redirect.htm?keyword="jlca1005"'
			private Log log;
		}
		
		internal class TUNING_PENDING_SessionOperations : SessionOperations
		{
			public TUNING_PENDING_SessionOperations()
			{
			}

			public virtual void changeState(SessionImpl s, core.SessionState newState)
			{
				if (!((newState == core.SessionState.SESSION_STATE_ACTIVE) || (newState == core.SessionState.SESSION_STATE_TUNING) || (newState == core.SessionState.SESSION_STATE_ABORTED)))
				{
					throw new BEEPException("Illegal session state transition");
				}
				
				s.state = newState;
			}
			
			public virtual bool postFrame(SessionImpl s, Frame f)
			{
				return ((ChannelImpl) f.Channel).postFrame(f);
			}
			
			private Log log = LogFactory.getLog(typeof(TUNING_PENDING_SessionOperations));
		}
		
		internal class TUNING_SessionOperations : SessionOperations
		{
			public virtual void  changeState(SessionImpl s, core.SessionState newState)
			{
				if (!((newState == core.SessionState.SESSION_STATE_CLOSED) || (newState == core.SessionState.SESSION_STATE_ABORTED)))
				{
					throw new BEEPException("Illegal session state transition");
				}
				
				s.state = newState;
			}
			
			public virtual bool postFrame(SessionImpl s, Frame f)
			{
				return ((ChannelImpl) f.Channel).postFrame(f);
			}
			
			private Log log = LogFactory.getLog(typeof(TUNING_SessionOperations));
		}
		
		internal class CLOSE_PENDING_SessionOperations : SessionOperations
		{
			public virtual void  changeState(SessionImpl s, core.SessionState newState)
			{
				if (!((newState == core.SessionState.SESSION_STATE_ACTIVE) || (newState == core.SessionState.SESSION_STATE_CLOSING) || (newState == core.SessionState.SESSION_STATE_ABORTED)))
				{
					throw new BEEPException("Illegal session state transition");
				}
				
				s.state = newState;
			}
			
			public virtual bool postFrame(SessionImpl s, Frame f)
			{
				// If we're in an error state
				log.debug("Dropping a frame because the Session state is " + "no longer active.");
				return false;
			}
			
			private Log log = LogFactory.getLog(typeof(CLOSE_PENDING_SessionOperations));
		}
		
		internal class CLOSING_SessionOperations : SessionOperations
		{
			public virtual void changeState(SessionImpl s, core.SessionState newState)
			{
				if (!((newState == core.SessionState.SESSION_STATE_CLOSED) || (newState == core.SessionState.SESSION_STATE_ABORTED)))
				{
					throw new BEEPException("Illegal session state transition");
				}
				
				s.state = newState;
			}
			
			public virtual bool postFrame(SessionImpl s, Frame f)
			{
				return ((ChannelImpl) f.Channel).postFrame(f);
			}
			
			private Log log = LogFactory.getLog(typeof(CLOSING_SessionOperations));
		}
		
		internal class CLOSED_SessionOperations : SessionOperations
		{
			public virtual void  changeState(SessionImpl s, core.SessionState newState)
			{
				if (newState == core.SessionState.SESSION_STATE_ABORTED)
				{
					log.info("Error aborting, session already in a closed state.");
				}
				else if (newState == core.SessionState.SESSION_STATE_CLOSE_PENDING)
				{
					log.info("Error changing state to close pending, session already in a closed state.");
				}
				else
				{
					throw new BEEPException("Illegal session state transition (" + newState + ")");
				}
			}
			
			public virtual bool postFrame(SessionImpl s, Frame f)
			{
				// If we're in an error state
				log.debug("Dropping a frame because the Session state is " + "no longer active.");
				return false;
			}
			
			private Log log = LogFactory.getLog(typeof(CLOSED_SessionOperations));
		}
		
		internal class ABORTED_SessionOperations : SessionOperations
		{
			public virtual void changeState(SessionImpl s, core.SessionState newState)
			{
				throw new BEEPException("Illegal session state transition");
			}
			
			public virtual bool postFrame(SessionImpl s, Frame f)
			{
				// If we're in an error state
				log.debug("Dropping a frame because the Session state is " + "no longer active.");
				return false;
			}
			
			private Log log = LogFactory.getLog(typeof(ABORTED_SessionOperations));
		}

	}
}