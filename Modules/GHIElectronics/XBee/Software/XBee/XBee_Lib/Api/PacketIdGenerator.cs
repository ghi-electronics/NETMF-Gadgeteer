using System;

namespace NETMF.OpenSource.XBee.Api
{
    /// <summary>
    /// Generates packet id numbers to be used with XBeeRequest.
    /// </summary>
    public class PacketIdGenerator
    {
        /// <summary>
        /// XBee will not generate a TX Status Packet if this frame id sent
        /// </summary>
        public const byte NoResponseId = 0;

        /// <summary>
        /// XBee will send a response to the request
        /// </summary>
        /// <remarks>
        /// This value is used if the value is not generated
        /// </remarks>
        public const byte DefaultId = 1;

        private byte _currentId;
        private readonly byte _maxId;
        private readonly byte _minId;

        public PacketIdGenerator(byte minId = 2, byte maxId = 0xFF)
        {
            if (minId > maxId || minId == 0)
                throw new ArgumentOutOfRangeException("minId");

            _minId = minId;
            _maxId = maxId;
            _currentId = minId;
        }

        /// <summary>
        /// Generates an id for XBeeRequest packet.
        /// </summary>
        /// <returns>Next value in sequence between given minId and maxId</returns>
        public byte GetNext()
        {
            if (_currentId == _maxId)
            {
                _currentId = _minId;
            }
            else
            {
                _currentId++;
            }

            return _currentId;  
        }

        /// <summary>
        /// Updates the packet id.
        /// </summary>
        /// <param name="newId">Any value between minId and maxId is valid</param>
        public void Update(byte newId)
        {
            if (newId < _minId || newId > _maxId)
                throw new ArgumentException("invalid id");

            _currentId = newId;
        }
    }
}