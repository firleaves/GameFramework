//------------------------------------------------------------
// Game Framework
// Copyright © 2013-2021 Jiang Yin. All rights reserved.
// Homepage: https://gameframework.cn/
// Feedback: mailto:ellan@gameframework.cn
//------------------------------------------------------------

using System;
using System.IO;

namespace GameFramework.Network
{
    internal sealed partial class NetworkManager : GameFrameworkModule, INetworkManager
    {
        private sealed class ReceiveState : IDisposable
        {
            private const int DefaultBufferLength = 32767;
            private MemoryStream m_Stream;
            private IPacketHeader m_PacketHeader;
            private bool m_Disposed;

            private MemoryStream m_CacheReceiveStream = null;

            public ushort Header
            {
                get;
                set;
            } = 0;
            public bool WaitingNextPacket 
            {
                get;set;
            }
            public ReceiveState()
            {
                m_Stream = new MemoryStream(DefaultBufferLength);
                //m_CacheReceiveStream = new MemoryStream(DefaultBufferLength);
                m_PacketHeader = null;
                m_Disposed = false;
            }

            public MemoryStream Stream
            {
                get
                {
                    return m_Stream;
                }
            }

            public IPacketHeader PacketHeader
            {
                get
                {
                    return m_PacketHeader;
                }
            }

            public void PrepareForPacketHeader(int packetHeaderLength)
            {
                Reset(packetHeaderLength, null);
            }


            /// <summary>
            /// 保存stream里面数据
            /// </summary>
            public void SaveStreamData()
            {
                //GameFrameworkLog.Warning("SaveStreamData");
                if (m_CacheReceiveStream==null)
                {
                    
                    m_CacheReceiveStream = new MemoryStream(DefaultBufferLength);
                }
                else
                {
                    m_CacheReceiveStream.Capacity = m_CacheReceiveStream.Capacity + DefaultBufferLength;
                }
                
                m_CacheReceiveStream.Write(Stream.GetBuffer(), (int)Stream.Position, (int)(Stream.Length - Stream.Position));
                //m_Stream.Capacity = m_Stream.Capacity+size;
                //GameFrameworkLog.Warning("开始等待下一个包");
                //Reset(packetHeaderLength, null);

            }
            
            /// <summary>
            /// 讲缓存的stream数据转移到stream内,并重新计算长度,并且清理缓存stream
            /// </summary>
            public void TransferStream()
            {
                if(m_CacheReceiveStream!=null && m_CacheReceiveStream.Length > 0)
                {
                    
                    m_Stream.Write(m_CacheReceiveStream.ToArray(), 0, (int)m_CacheReceiveStream.Length);
                    m_Stream.Position = 0L;
                    m_PacketHeader.PacketLength = (int)m_Stream.Length;
                    //GameFrameworkLog.Warning($"合并大包{m_CacheReceiveStream.Length}");
                    m_CacheReceiveStream.Dispose();
                    m_CacheReceiveStream = null;
                }
                
            }
            
            public void PrepareForPacket(IPacketHeader packetHeader)
            {
                if (packetHeader == null)
                {
                    throw new GameFrameworkException("Packet header is invalid.");
                }
               
                Reset(packetHeader.PacketLength, packetHeader);
            }

            public void Dispose()
            {
                Dispose(true);
                GC.SuppressFinalize(this);
            }

            private void Dispose(bool disposing)
            {
                if (m_Disposed)
                {
                    return;
                }

                if (disposing)
                {
                    if (m_Stream != null)
                    {
                        m_Stream.Dispose();
                        m_Stream = null;
                    }
                    if(m_CacheReceiveStream!=null)
                    {
                        m_CacheReceiveStream.Dispose();
                        m_CacheReceiveStream = null;
                    }
                }

                m_Disposed = true;
            }

            private void Reset(int targetLength, IPacketHeader packetHeader)
            {
                if (targetLength < 0)
                {
                    throw new GameFrameworkException("Target length is invalid.");
                }
                
                m_Stream.Position = 0L;
                m_Stream.SetLength(targetLength);
                m_PacketHeader = packetHeader;
            }
        }
    }
}
