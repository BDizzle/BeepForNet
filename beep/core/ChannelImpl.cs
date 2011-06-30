/*
* ChannelImpl.java  $Revision: 1.1 $ $Date: 2004/09/21 08:46:56 $
*
* Copyright (c) 2001 Invisible Worlds, Inc.  All rights reserved.
* Copyright (c) 2001-2003 Huston Franklin.  All rights reserved.
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
using System.Threading;
using Log = beepcore.logging.Log;
using LogFactory = beepcore.logging.LogFactory;
using BufferSegment = beepcore.beep.util.BufferSegment;
namespace beepcore.beep.core
{
	/// <summary>ChannelImpl is a conduit for a certain kind of traffic over a session,
	/// determined by the profile of the channel.  Channels are created by Session</summary>
	/// <author>Eric Dixon</author>
	/// <author>Huston Franklin</author>
	/// <author>Jay Kint</author>
	/// <author>Scott Pead</author>
	/// <version>$Revision: 1.1 $, $Date: 2004/09/21 08:46:56 $</version>
	class ChannelImpl : IChannel
	{
		private void  InitBlock()
		{
			log = LogFactory.getLog(this.GetType());
			state = core.ChannelState.STATE_INITIALIZED;
		}

		/// <summary>Returns or sets application context data.</summary>
		virtual public object AppData
		{
			get
			{
				return this.applicationData;
			}
			
			set
			{
				this.applicationData = value;
			}
			
		}
		/// <summary>Returns the receive buffer size for this channel.</summary>
		//UPGRADE_NOTE: Synchronized keyword was removed from method 'getBufferSize'. Lock expression was added. 'ms-help://MS.VSCC.2003/commoner/redir/redirect.htm?keyword="jlca1027"'
		virtual public int BufferSize
		{
			get
			{
				lock (this)
				{
					return recvWindowSize;
				}
			}
			
		}
		/// <summary>Returns the encoding used on this <code>Channel</code>
		/// @todo look at removing this and adding the information to getProfile()</summary>
		virtual internal string Encoding
		{
			get
			{
				return encoding;
			}
			
			set
			{
				this.encoding = value;
			}
			
		}
		/// <summary>Return the number of this <code>Channel</code>.</summary>
		virtual public int Number
		{
			get
			{
				return System.Int32.Parse(number);
			}
			
		}
		/// <summary>Sets the receive buffer size for this channel.  Default size is 4K.</summary>
		/// <param name="size"></param>
		/// <exception cref="BEEPException" />
		virtual public int ReceiveBufferSize
		{
			set
			{
				lock (this)
				{
					if ((state != core.ChannelState.STATE_ACTIVE) && (state != core.ChannelState.STATE_INITIALIZED))
					{
						throw new BEEPException("Channel in a bad state.");
					}
					
					// make sure we aren't setting the size less than what is currently
					// in the buffer right now.
					if (value < recvWindowUsed)
					{
						throw new BEEPException("New size is less than what is " + "currently in use.");
					}
					
					// set the new size and copy the buffer
					recvWindowSize = value;
					
					if (log.isDebugEnabled())
					{
						log.debug("Buffer size for channel " + number + " set to " + recvWindowSize);
					}
					
					sendWindowUpdate();
				}
			}
			
		}
		/// <summary>Returns the session for this channel.</summary>
		virtual public ISession Session
		{
			get
			{
				return this.session;
			}
			
		}
		/// <summary>get the number of this <code>Channel</code> as a <code>String</code></summary>
		virtual internal string NumberAsString
		{
			get
			{
				return number;
			}
			
		}
		private string StateString
		{
			get
			{
				switch (state)
				{
					
					case core.ChannelState.STATE_INITIALIZED: 
						return "initialized";
					
					case core.ChannelState.STATE_STARTING: 
						return "starting";
					
					case core.ChannelState.STATE_ACTIVE: 
						return "active";
					
					case core.ChannelState.STATE_TUNING_PENDING: 
						return "tuning pending";
					
					case core.ChannelState.STATE_TUNING: 
						return "tuning";
					
					case core.ChannelState.STATE_CLOSE_PENDING: 
						return "close pending";
					
					case core.ChannelState.STATE_CLOSING: 
						return "closing";
					
					case core.ChannelState.STATE_CLOSED: 
						return "closed";
					
					case core.ChannelState.STATE_ABORTED: 
						return "aborted";
					
					default: 
						return "unknown";
					
				}
			}
			
		}

		/// <summary>Method getAvailableWindow
		/// This is called from Session to provide a # used
		/// to screen frame sizes against and enforce the
		/// protocol.
		/// </summary>
		/// <returns>int the amount of free buffer space
		/// available.</returns>
		virtual internal int AvailableWindow
		{
			get
			{
				lock (this)
				{
					return (recvWindowSize - recvWindowUsed);
				}
			}
			
		}

		/// <summary>Used to get/set data that can be piggybacked on
		/// a profile reply to a start channel request
		/// (or any other scenario we choose)
		/// 
		/// Could be called by users, profile implementors etc.
		/// to fetch data off a profile response.
		/// Called by Channel Zero</summary>
		virtual public string StartData
		{
			get
			{
				return startData;
			}
			
			set
			{
				startData = value;
			}
			
		}

		private static readonly BufferSegment zeroLengthSegment = new BufferSegment(new byte[0]);
		
		//UPGRADE_NOTE: There is an untranslated Statement.  Please refer to original code. 'ms-help://MS.VSCC.2003/commoner/redir/redirect.htm?keyword="jlca1153"'
		
		/// <summary>@todo check this </summary>
		
		// default values for some variables
		internal const int DEFAULT_WINDOW_SIZE = 4096;
		
		//UPGRADE_NOTE: Final was removed from the declaration of 'defaultHandler '. 'ms-help://MS.VSCC.2003/commoner/redir/redirect.htm?keyword="jlca1003"'
		internal static readonly IRequestHandler defaultHandler = new DefaultMSGHandler();
		
		// instance variables
		
		//UPGRADE_NOTE: The initialization of  'log' was moved to method 'InitBlock'. 'ms-help://MS.VSCC.2003/commoner/redir/redirect.htm?keyword="jlca1005"'
		private Log log;
		
		/// <summary>syntax of messages </summary>
		private string profile;
		
		/// <summary>encoding of used by profile </summary>
		private string encoding;
		
		/// <summary>channel number on the session </summary>
		private string number;
		
		/// <summary>Used to pass data sent on the Start Channel request </summary>
		private string startData;
		
		/// <summary>receiver of MSG messages </summary>
		private IRequestHandler handler;
		
		/// <summary>number of last message sent </summary>
		private int lastMessageSent;
		
		/// <summary>sequence number for messages sent </summary>
		private long sentSequence;
		
		/// <summary>sequence for messages received </summary>
		private long recvSequence;
		
		/// <summary>messages waiting for replies </summary>
		//UPGRADE_ISSUE: Class hierarchy differences between ''java.util.List'' and ''SupportClass.ListCollectionSupport'' may cause compilation errors. 'ms-help://MS.VSCC.2003/commoner/redir/redirect.htm?keyword="jlca1186"'
		private ArrayList sentMSGQueue;
		
		/// <summary>MSG we've received by awaiting proceesing of a former MSG </summary>
		private ArrayList recvMSGQueue;
		
		/// <summary>messages queued to be sent </summary>
		private ArrayList pendingSendMessages;
		
		/// <summary>session this channel sends through. </summary>
		private SessionImpl session;
		
		/// <summary>message that we are receiving frames </summary>
		private ArrayList recvReplyQueue;
		
		//UPGRADE_NOTE: The initialization of  'state' was moved to method 'InitBlock'. 'ms-help://MS.VSCC.2003/commoner/redir/redirect.htm?keyword="jlca1005"'
		private core.ChannelState state;
		
		private Frame previousFrame;
		
		/// <summary>size of the peer's receive buffer </summary>
		private int peerWindowSize;
		
		/// <summary>size of the receive buffer </summary>
		private int recvWindowSize;
		
		/// <summary>amount of the buffer in use </summary>
		private int recvWindowUsed;
		
		private int recvWindowFreed;
		
		private object applicationData = null;
		
		// tuningProfile indicates that the profile for this channel will
		// request a tuning reset
		private bool tuningProfile = false;
		
		/* (non-Javadoc)
		* @see java.lang.Object#toString()
		*/
		public override string ToString()
		{
			return base.ToString() + " (#" + NumberAsString + " " + StateString + " on " + session.ToString() + ")";
		}
		
		internal ChannelImpl(string profile, string number, IRequestHandler handler, bool tuningReset, SessionImpl session)
		{
			InitBlock();
			this.profile = profile;
			this.encoding = core.StringConstants.ENCODING_DEFAULT;
			this.number = number;
			this.setRequestHandler(handler, tuningReset);
			this.session = session;
			sentSequence = 0;
			recvSequence = 0;
			// BUGFIX non-zero channels have no reason to start at 1 : they have no greeting as MSG 0
			lastMessageSent = 0;
			
			pendingSendMessages = new ArrayList();
			sentMSGQueue = ArrayList.Synchronized(new ArrayList());
			recvMSGQueue = new ArrayList();
			recvReplyQueue = new ArrayList();
			state = core.ChannelState.STATE_INITIALIZED;
			recvWindowUsed = 0;
			recvWindowFreed = 0;
			recvWindowSize = DEFAULT_WINDOW_SIZE;
			peerWindowSize = DEFAULT_WINDOW_SIZE;
		}
		
		internal ChannelImpl(string profile, string number, SessionImpl session):this(profile, number, defaultHandler, false, session)
		{
		}
		
		internal static ChannelImpl createChannelZero(SessionImpl session, ReplyListener reply, IRequestHandler handler)
		{
			ChannelImpl channel = new ChannelImpl(null, "0", handler, true, session);
			
			// Add a MSG to the SentMSGQueue to fake channel into accepting the
			// greeting which comes in an unsolicited RPY.
			//UPGRADE_TODO: The equivalent in .NET for method 'java.util.List.add' may return a different value. 'ms-help://MS.VSCC.2003/commoner/redir/redirect.htm?keyword="jlca1043"'
			channel.sentMSGQueue.Add(new MessageStatus(channel, MessageType.MESSAGE_TYPE_MSG, 0, null, reply));
			channel.recvMSGQueue.Add(new MessageMSGImpl(channel, 0, null));
			// BUGFIX count the greeting as MSG 0
			channel.lastMessageSent++;
			
			channel.state = core.ChannelState.STATE_ACTIVE;
			
			return channel;
		}
		
		/// <summary>Closes the channel.</summary>
		/// <exception cref="BEEPException" />
		public virtual void  close()
		{
			
			// @todo the other BEEP peer may refuse this request
			// should we return a boolean or throw a CloseChannelException?
			session.closeChannel(this, BEEPStatusCode.SUCCESS, null);
		}
		
		// instance methods
		
#if MESSAGELISTENER
		/// <summary>Sets the <code>MessageListener</code> for this channel.</summary>
		/// <param name="ml"></param>
		/// <returns>The previous MessageListener or null if none was set.</returns>
		public virtual MessageListener setMessageListener(MessageListener ml)
		{
			MessageListener tmp = getMessageListener();
			
			this.handler = new MessageListenerAdapter(ml);
			
			return tmp;
		}
		
		/// <summary>Returns the message listener for this channel.</summary>
		public virtual MessageListener getMessageListener()
		{
			if (!(this.handler is MessageListenerAdapter))
			{
				return null;
			}
			
			return ((MessageListenerAdapter) this.handler).MessageListener;
		}
#endif

		/// <summary>Returns the <code>RequestHandler</code> registered with this channel.</summary>
		public virtual IRequestHandler getRequestHandler()
		{
			return this.handler;
		}
		
		/// <summary>Sets the MSG handler for this <code>Channel</code>.</summary>
		/// <param name="handler"><code>RequestHandler</code> to handle received MSG messages.</param>
		/// <returns>The previous <code>RequestHandler</code> or <code>null</code> if
		/// one wasn't set.</returns>
		public virtual IRequestHandler setRequestHandler(IRequestHandler handler)
		{
			return this.setRequestHandler(handler, false);
		}
		
		/// <summary>Sets the MSG handler for this <code>Channel</code>.</summary>
		/// <param name="handler"><code>RequestHandler</code> to handle received MSG messages.</param>
		/// <param name="tuningReset">flag indicating that the profile will request a
		/// tuning reset.</param>
		/// <returns>The previous <code>RequestHandler</code> or <code>null</code> if
		/// one wasn't set.</returns>
		public virtual IRequestHandler setRequestHandler(IRequestHandler handler, bool tuningReset)
		{
			IRequestHandler tmp = this.handler;
			
			this.handler = handler;
			this.tuningProfile = tuningReset;
			
			return tmp;
		}
		
		public virtual void DispatchMSG(object _unused)
		{
			MessageMSGImpl m;
			lock (recvMSGQueue)
			{
				m = (MessageMSGImpl) recvMSGQueue[0];
				lock (m)
				{
					m.setNotified();
				}
			}
			
			handler.ReceiveMSG(m);
		}
		
		/// <summary>Sends a message of type MSG.</summary>
		/// <param name="stream">Data to send in the form of <code>DataStream</code>.</param>
		/// <param name="replyListener">A "one-shot" listener that will handle replies
		/// to this sendMSG listener.</param>
		/// <seealso cref="OutputDataStream"></seealso>
		/// <seealso cref="MessageStatus"></seealso>
		/// <returns>MessageStatus</returns>
		/// <exception cref="BEEPException">if an error is encoutered.</exception>
		public virtual MessageStatus sendMSG(OutputDataStream stream, ReplyListener replyListener)
		{
			MessageStatus status;
			
			if (state != core.ChannelState.STATE_ACTIVE && state != core.ChannelState.STATE_TUNING)
			{
				switch (state)
				{
					
					case core.ChannelState.STATE_INITIALIZED: 
						throw new BEEPException("Channel is uninitialised.");
					
					default: 
						throw new BEEPException("Channel is in an unknown state.");
					
				}
			}
			
			lock (this)
			{
				
				// create a new request
				status = new MessageStatus(this, MessageType.MESSAGE_TYPE_MSG, lastMessageSent, stream, replyListener);
				
				// BUGFIX message 0 was NOT NECESSARILY the greeting
				++lastMessageSent;
			}
			
			// put this in the list of messages waiting
			// may want to put an expiration or something in here so they
			// don't just stay around taking up space.
			// @todo it's a synchronized list, you don't have to sync
			lock (sentMSGQueue)
			{
				sentMSGQueue.Add(status);
			}
			
			// send it on the session
			sendToPeer(status);
			
			return status;
		}
		
		internal virtual void abort()
		{
			setState(core.ChannelState.STATE_ABORTED);
		}
		
		internal virtual void addPiggybackedMSG(PiggybackedMSG msg)
		{
			recvMSGQueue.Add(msg);
			try
			{
				ThreadPool.QueueUserWorkItem(new WaitCallback(this.DispatchMSG));
			}
			catch (System.Threading.ThreadInterruptedException e)
			{
				/// <summary>@TODO handle this better </summary>
				throw new BEEPException(e);
			}
		}
		
		public virtual core.ChannelState getState()
		{
			return state;
		}
		
		private void  receiveFrame(Frame frame)
		{
			
			// if this is an incoming message rather than a reply to a
			// previously sent message
			if (frame.MessageType == MessageType.MESSAGE_TYPE_MSG)
			{
				lock (recvMSGQueue)
				{
					MessageMSGImpl m = null;
					if (recvMSGQueue.Count != 0)
					{
						m = (MessageMSGImpl) recvMSGQueue[recvMSGQueue.Count-1];
						
						if (m.Msgno != frame.Msgno)
						{
							m = null;
						}
					}
					
					if (m != null)
					{
						/// Move this code to DataStream...
						lock (m)
						{
							foreach(BufferSegment bs in frame.Payload)
							{
								m.DataStream.add(bs);
							}
							
							if (frame.isLast())
							{
								m.DataStream.setComplete();
							}
						}
						
						return ;
					}
					
					m = new MessageMSGImpl(this, frame.Msgno, new InputDataStream(this));
					
					m.setNotified();
					
					foreach(BufferSegment bs in frame.Payload)
					{
						m.DataStream.add(bs);
					}
					
					if (frame.isLast())
					{
						m.DataStream.setComplete();
					}
					
					recvMSGQueue.Add(m);
					
					if (recvMSGQueue.Count == 1)
					{
						try
						{
							ThreadPool.QueueUserWorkItem(new System.Threading.WaitCallback(this.DispatchMSG));
						}
						catch (System.Threading.ThreadInterruptedException e)
						{
							/// <summary>@TODO handle this better </summary>
							throw new BEEPException(e);
						}
					}
				}
				
				return ;
			}
			
			MessageImpl m2 = null;
			
			// This frame must be for a reply (RPY, ERR, ANS, NUL)
			MessageStatus mstatus;
			
			// Find corresponding MSG for this reply
			lock (sentMSGQueue)
			{
				//IMessage sentMSG;
				
				if (sentMSGQueue.Count == 0)
				{
					
					// @todo shutdown session (we think)
				}
				
				mstatus = (MessageStatus) sentMSGQueue[0];
				
				if (mstatus.Msgno != frame.Msgno)
				{
					
					// @todo shutdown session (we think)
				}
				
				// If this is the last frame for the reply (NUL, RPY, or
				// ERR) to this MSG.
				if ((frame.isLast() == true) && (frame.MessageType != MessageType.MESSAGE_TYPE_ANS))
				{
					sentMSGQueue.RemoveAt(0);
				}
			}
			
			ReplyListener replyListener = mstatus.ReplyListener;
			
			// error if they don't have either a frame or reply listener
			if (replyListener == null)
			{
				
				// @todo should we check this on sendMSG instead?
			}
			
			if (frame.MessageType == MessageType.MESSAGE_TYPE_NUL)
			{
				lock (recvReplyQueue)
				{
					if (recvReplyQueue.Count != 0)
					{
						
						// There are ANS messages on the queue for which we
						// haven't received the last frame.
						log.debug("Received NUL before last ANS");
						session.terminate("Received NUL before last ANS");
					}
				}
				
				m2 = new MessageImpl(this, frame.Msgno, null, MessageType.MESSAGE_TYPE_NUL);
				
				mstatus.MessageStatusCode = core.MessageStatusCode.MESSAGE_STATUS_RECEIVED_REPLY;
				if (log.isDebugEnabled())
				{
					//UPGRADE_TODO: The equivalent in .NET for method 'java.lang.Object.toString' may return a different value. 'ms-help://MS.VSCC.2003/commoner/redir/redirect.htm?keyword="jlca1043"'
					log.debug("Notifying reply listener =>" + replyListener + "for NUL message");
				}
				
				replyListener.receiveNUL(m2);
				
				return ;
			}
			
			// is this an ANS message?
			if (frame.MessageType == MessageType.MESSAGE_TYPE_ANS)
			{
				
				// see if this answer number has already come in
				lock (recvReplyQueue)
				{
					int i;
					
					m2 = null;
					
					for(i=0;i<recvReplyQueue.Count;i++)
					{
						MessageImpl tmp = (MessageImpl) recvReplyQueue[i];
						
						if (tmp.Ansno == frame.Ansno)
						{
							m2 = tmp;
							
							break;
						}
					}
					
					// if no answer was found, then create a new one and
					// add it to the queue
					if (m2 == null)
					{
						m2 = new MessageImpl(this, frame.Msgno, frame.Ansno, new InputDataStream(this));
						
						if (!frame.isLast())
						{
							recvReplyQueue.Add(m2);
						}
					}
					else if (frame.isLast())
					{
						// remove the found ANS from the recvReplyQueue
						recvReplyQueue.RemoveAt(i);
					}
				}
			}
			else
			{
				// ERR or RPY
				lock (recvReplyQueue)
				{
					if (recvReplyQueue.Count == 0)
					{
						m2 = new MessageImpl(this, frame.Msgno, new InputDataStream(this), frame.MessageType);
						
						if (frame.isLast() == false)
						{
							recvReplyQueue.Add(m2);
						}
					}
					else
					{
						
						// @todo sanity check: make sure this is the
						// right Message
						m2 = (MessageImpl) recvReplyQueue[0];
						
						if (frame.isLast())
						{
							recvReplyQueue.RemoveAt(0);
						}
					}
					
					if (frame.isLast())
					{
						if (frame.MessageType == MessageType.MESSAGE_TYPE_ERR)
						{
							mstatus.MessageStatusCode = core.MessageStatusCode.MESSAGE_STATUS_RECEIVED_ERROR;
						}
						else
						{
							mstatus.MessageStatusCode = core.MessageStatusCode.MESSAGE_STATUS_RECEIVED_REPLY;
						}
					}
				}
			}
			
			foreach(BufferSegment bs in frame.Payload)
			{
				m2.DataStream.add(bs);
			}
			
			if (frame.isLast())
			{
				m2.DataStream.setComplete();
			}
			
			// notify message listener if this message has not been notified before
			lock (m2)
			{
				if (m2.isNotified())
				{
					return ;
				}
				
				m2.setNotified();
			}
			
			if (log.isDebugEnabled())
			{
				log.debug("Notifying reply listener.=>" + replyListener);
			}
			
			if (m2.messageType == MessageType.MESSAGE_TYPE_RPY)
			{
				replyListener.receiveRPY(m2);
			}
			else if (m2.messageType == MessageType.MESSAGE_TYPE_ERR)
			{
				replyListener.receiveERR(m2);
			}
			else if (m2.messageType == MessageType.MESSAGE_TYPE_ANS)
			{
				replyListener.receiveANS(m2);
			}
		}
		
		/// <summary>interface between the session.  The session receives a frame and then
		/// calls this function.  The function then calls the message listener
		/// via some intermediary thread functions.  The message hasn't been
		/// completely received.  The data stream contained in the message will
		/// block if more is expected.</summary>
		/// <param name="frame">- the frame received by the session</param>
		internal virtual bool postFrame(Frame frame)
		{
			log.trace("Channel::postFrame");
			
			if (state != core.ChannelState.STATE_ACTIVE && state != core.ChannelState.STATE_TUNING)
			{
				throw new BEEPException("State is " + state);
			}
			
			validateFrame(frame);
			
			recvSequence += frame.Size;
			
			// subtract this from the amount available in the buffer
			recvWindowUsed += frame.Size;
			
			// make sure we didn't overflow the buffer
			if (recvWindowUsed > recvWindowSize)
			{
				throw new BEEPException("Channel window overflow");
			}
			
			receiveFrame(frame);
			
			if (frame.MessageType == MessageType.MESSAGE_TYPE_MSG)
			{
				return !(frame.isLast() && tuningProfile);
			}
			else
			{
				return !(frame.isLast() && getState() == core.ChannelState.STATE_TUNING);
			}
		}
		
		internal virtual void  sendMessage(MessageStatus m)
		{
			if (state != core.ChannelState.STATE_ACTIVE && state != core.ChannelState.STATE_TUNING)
			{
				switch (state)
				{
					
					case core.ChannelState.STATE_INITIALIZED: 
						throw new BEEPException("Channel is uninitialised.");
					
					default: 
						throw new BEEPException("Channel is in an unknown state.");
					
				}
			}
			
			// send it on the session
			sendToPeer(m);
		}
		
		private void  sendToPeer(MessageStatus status)
		{
			lock (pendingSendMessages)
			{
				//UPGRADE_TODO: The equivalent in .NET for method 'java.util.List.add' may return a different value. 'ms-help://MS.VSCC.2003/commoner/redir/redirect.htm?keyword="jlca1043"'
				pendingSendMessages.Add(status);
			}
			status.MessageData.Channel = this;
			sendQueuedMessages();
		}
		
		//UPGRADE_NOTE: Synchronized keyword was removed from method 'sendQueuedMessages'. Lock expression was added. 'ms-help://MS.VSCC.2003/commoner/redir/redirect.htm?keyword="jlca1027"'
		internal virtual void  sendQueuedMessages()
		{
			lock (this)
			{
				while (true)
				{
					MessageStatus status;
					
					lock (pendingSendMessages)
					{
						if (pendingSendMessages.Count==0)
						{
							return ;
						}
						status = (MessageStatus) pendingSendMessages[0];
						pendingSendMessages.RemoveAt(0);
					}
					
					if (this.recvWindowFreed != 0)
					{
						sendWindowUpdate();
					}
					
					sendFrames(status);
					
					if (status.MessageStatusCode != core.MessageStatusCode.MESSAGE_STATUS_SENT)
					{
						lock (pendingSendMessages)
						{
							pendingSendMessages.Insert(0, status);
						}
						return ;
					}
				}
			}
		}
		
		private void  sendFrames(MessageStatus status)
		{
			int sessionBufferSize = session.MaxFrameSize;
			OutputDataStream ds = status.MessageData;
			
			do 
			{
				lock (this)
				{
					// create a frame
					Frame frame = new Frame(status.MessageType, this, status.Msgno, false, sentSequence, 0, status.Ansno);
					
					// make sure the other peer can accept something
					if (peerWindowSize == 0)
					{
						return ;
					}
					
					int maxToSend = System.Math.Min(sessionBufferSize, peerWindowSize);
					
					int size = 0;
					while (size < maxToSend)
					{
						if (ds.availableSegment() == false)
						{
							if (size == 0)
							{
								if (ds.isComplete() == false)
								{
									// More BufferSegments are expected...
									return ;
								}
								
								frame.addPayload(zeroLengthSegment);
							}
							
							// Send what we have
							break;
						}
						
						BufferSegment b = ds.getNextSegment(maxToSend - size);
						
						frame.addPayload(b);
						
						size += b.Length;
					}
					
					if (ds.isComplete() && ds.availableSegment() == false)
					{
						frame.setLast();
					}
					
					try
					{
						session.sendFrame(frame);
					}
					catch (BEEPException e)
					{
						/*
						* @todo we should do something more than just log
						* the error (e.g. close the channel or session).
						*/
						log.error("sendFrames", e);
						status.MessageStatusCode = core.MessageStatusCode.MESSAGE_STATUS_NOT_SENT;
						
						throw e;
					}
					
					// update the sequence and peer window size
					sentSequence += size;
					peerWindowSize -= size;
				}
			}
			while (ds.availableSegment() == true || ds.isComplete() == false);
			
			status.MessageStatusCode = core.MessageStatusCode.MESSAGE_STATUS_SENT;
			
			if (ds.isComplete() && ds.availableSegment() == false && (status.MessageType == MessageType.MESSAGE_TYPE_RPY || status.MessageType == MessageType.MESSAGE_TYPE_ERR || status.MessageType == MessageType.MESSAGE_TYPE_NUL))
			{
				MessageMSGImpl m;
				lock (recvMSGQueue)
				{
					recvMSGQueue.RemoveAt(0);
					
					if (recvMSGQueue.Count != 0)
					{
						m = (MessageMSGImpl) recvMSGQueue[0];
						lock (m)
						{
							m.setNotified();
						}
					}
					else
					{
						m = null;
					}
				}
				
				if (m != null)
				{
					ThreadPool.QueueUserWorkItem(new WaitCallback(this.DispatchMSG));
				}
			}
		}
		
		private void  sendWindowUpdate()
		{
			if (session.updateMyReceiveBufferSize(this, recvSequence, recvWindowSize - (recvWindowUsed - recvWindowFreed)))
			{
				recvWindowUsed -= recvWindowFreed;
				recvWindowFreed = 0;
			}
		}
		
		/// <summary>Method setState</summary>
		/// <param name="newState"></param>
		/// <exception cref="BEEPException" />
		internal virtual void setState(core.ChannelState newState)
		{
			lock (this)
			{
				log.trace("CH" + number + " state=" + newState);
				
				this.state = newState;
				
				/// @todo state transition rules and error checking
				//if (false)
				//{
				//	session.terminate("Bad state transition in channel");
				//}
			}
		}
		
		internal virtual void  setProfile(string profile)
		{
			this.profile = profile;
		}
		
		/// <summary>Returns the profile for this channel.</summary>
		public virtual string getProfile()
		{
			return this.profile;
		}
		
		//UPGRADE_NOTE: Synchronized keyword was removed from method 'updatePeerReceiveBufferSize'. Lock expression was added. 'ms-help://MS.VSCC.2003/commoner/redir/redirect.htm?keyword="jlca1027"'
		internal virtual void  updatePeerReceiveBufferSize(long lastSeq, int size)
		{
			lock (this)
			{
				int previousPeerWindowSize = peerWindowSize;
				
				if (log.isDebugEnabled())
				{
					log.debug("Channel.updatePeerReceiveBufferSize: size = " + size + " lastSeq = " + lastSeq + " sentSequence = " + sentSequence + " peerWindowSize = " + peerWindowSize);
				}
				
				peerWindowSize = size - (int) (sentSequence - lastSeq);
				
				log.debug("Channel.updatePeerReceiveBufferSize: New window size = " + peerWindowSize);
				
				if ((previousPeerWindowSize == 0) && (peerWindowSize > 0))
				{
					try
					{
						sendQueuedMessages();
					}
					catch (BEEPException)
					{
					}
				}
			}
		}
		
		private void  validateFrame(Frame frame)
		{
			lock (this)
			{
				
				if (previousFrame == null)
				{
					// is the message number correct?
					if (frame.MessageType == MessageType.MESSAGE_TYPE_MSG)
					{
						lock (recvMSGQueue)
						{
							for(int i=recvMSGQueue.Count-1;i>=0;i++) 
							{
								if (((IMessage) recvMSGQueue[i]).Msgno == frame.Msgno)
								{
									throw new BEEPException("Received a frame " + "with a duplicate " + "msgno (" + frame.Msgno + ")");
								}
							}
						}
					}
					else
					{
						MessageStatus mstatus;
						
						lock (sentMSGQueue)
						{
							if (sentMSGQueue.Count == 0)
							{
								throw new BEEPException("Received unsolicited reply");
							}
							
							mstatus = (MessageStatus) sentMSGQueue[0];
						}
						
						if (frame.Msgno != mstatus.Msgno)
						{
							throw new BEEPException("Incorrect message number: was " + frame.Msgno + "; expecting " + mstatus.Msgno);
						}
					}
				}
				else
				{
					// is the message type the same as the previous frames?
					if (previousFrame.MessageType != frame.MessageType)
					{
						throw new BEEPException("Incorrect message type: was " + frame.MessageTypeString + "; expecting " + previousFrame.MessageTypeString);
					}
					
					// is the message number correct?
					if (frame.MessageType == MessageType.MESSAGE_TYPE_MSG && frame.Msgno != previousFrame.Msgno)
					{
						throw new BEEPException("Incorrect message number: was " + frame.Msgno + "; expecting " + previousFrame.Msgno);
					}
				}
				
				// is the sequence number correct?
				if (frame.Seqno != recvSequence)
				{
					throw new BEEPException("Incorrect sequence number: was " + frame.Seqno + "; expecting " + recvSequence);
				}
			}
			
			if (frame.MessageType != MessageType.MESSAGE_TYPE_MSG)
			{
				MessageStatus mstatus;
				
				lock (sentMSGQueue)
				{
					if (sentMSGQueue.Count == 0)
					{
						throw new BEEPException("Received unsolicited reply");
					}
					
					mstatus = (MessageStatus) sentMSGQueue[0];
					
					if (mstatus.Msgno != frame.Msgno)
					{
						throw new BEEPException("Received reply out of order");
					}
				}
			}
			
			// save the previous frame to compare message types
			if (frame.isLast())
			{
				previousFrame = null;
			}
			else
			{
				previousFrame = frame;
			}
		}
		
		internal virtual void  freeReceiveBufferBytes(int size)
		{
			lock (this)
			{
				if (log.isTraceEnabled())
				{
					log.trace("Freed up " + size + " bytes on channel " + number);
				}
				
				recvWindowFreed += size;
				
				if (log.isTraceEnabled())
				{
					log.trace("recvWindowUsed = " + recvWindowUsed + " recvWindowFreed = " + recvWindowFreed + " recvWindowSize = " + recvWindowSize);
				}
				
				if (state == core.ChannelState.STATE_ACTIVE)
				{
					try
					{
						sendWindowUpdate();
					}
					catch (BEEPException e)
					{
						
						// do nothing
						log.fatal("Error updating receive buffer size", e);
					}
				}
			}
		}
		
#if MESSAGELISTENER
		internal class MessageListenerAdapter : RequestHandler
		{
			private void  InitBlock()
			{
				log = LogFactory.getLog(this.GetType());
			}
			virtual public MessageListener MessageListener
			{
				get
				{
					return this.listener;
				}
				
			}
			internal MessageListenerAdapter(MessageListener listener)
			{
				this.listener = listener;
			}
			
			public virtual void  receiveMSG(IMessageMSG message)
			{
				try
				{
					listener.receiveMSG(message);
				}
				catch (BEEPError e)
				{
					try
					{
						message.sendERR(e);
					}
					catch (BEEPException e2)
					{
						log.error("Error sending ERR", e2);
					}
				}
				catch (AbortChannelException e)
				{
					try
					{
						message.Channel.close();
					}
					catch (BEEPException e2)
					{
						log.error("Error closing channel", e2);
					}
				}
			}
			
			//UPGRADE_NOTE: The initialization of  'log' was moved to method 'InitBlock'. 'ms-help://MS.VSCC.2003/commoner/redir/redirect.htm?keyword="jlca1005"'
			private Log log;
			private MessageListener listener;
		}
#endif

		private class DefaultMSGHandler : IRequestHandler
		{
			private Log log;

			public DefaultMSGHandler()
			{
				log = LogFactory.getLog(this.GetType());
			}

			public virtual void ReceiveMSG(IMessageMSG message)
			{
				log.error("No handler registered to process MSG received on " + "channel " + message.Channel.Number);
				try
				{
					message.sendERR(BEEPStatusCode.REQUESTED_ACTION_ABORTED, "No MSG handler registered");
				}
				catch (BEEPException e)
				{
					log.error("Error sending ERR", e);
				}
			}
		}
	}
}