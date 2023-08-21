/******************************************************************************
 *
 * The MIT License (MIT)
 *
 * MIConvexHull, Copyright (c) 2015 David Sehnal, Matthew Campbell
 * 
 * Permission is hereby granted, free of charge, to any person obtaining a copy
 * of this software and associated documentation files (the "Software"), to deal
 * in the Software without restriction, including without limitation the rights
 * to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 * copies of the Software, and to permit persons to whom the Software is
 * furnished to do so, subject to the following conditions:
 * 
 * The above copyright notice and this permission notice shall be included in
 * all copies or substantial portions of the Software.
 * 
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
 * THE SOFTWARE.
 *  
 *****************************************************************************/

using System;

namespace JustAssets.ColliderUtilityRuntime.MIConvexHull
{
    /// <summary>
    /// A helper class for object allocation/storage.
    /// This helps the GC a lot as it prevents the creation of about 75% of
    /// new face objects (in the case of ConvexFaceInternal). In the case of
    /// FaceConnectors and DefferedFaces, the difference is even higher (in most
    /// cases O(1) vs O(number of created faces)).
    /// </summary>
    internal class ObjectManager
    {
        /// <summary>
        /// The dimension
        /// </summary>
        private readonly int _dimension;
        /// <summary>
        /// The connector stack
        /// </summary>
        private FaceConnector _connectorStack;
        /// <summary>
        /// The deferred face stack
        /// </summary>
        private readonly SimpleList<DeferredFace> _deferredFaceStack;
        /// <summary>
        /// The empty buffer stack
        /// </summary>
        private readonly SimpleList<IndexBuffer> _emptyBufferStack;
        /// <summary>
        /// The face pool
        /// </summary>
        private ConvexFaceInternal[] _facePool;
        /// <summary>
        /// The face pool size
        /// </summary>
        private int _facePoolSize;
        /// <summary>
        /// The face pool capacity
        /// </summary>
        private int _facePoolCapacity;
        /// <summary>
        /// The free face indices
        /// </summary>
        private readonly IndexBuffer _freeFaceIndices;

        /// <summary>
        /// The hull
        /// </summary>
        private readonly ConvexHullAlgorithm _hull;

        /// <summary>
        /// Create the manager.
        /// </summary>
        /// <param name="hull">The hull.</param>
        public ObjectManager(ConvexHullAlgorithm hull)
        {
            _dimension = hull.NumOfDimensions;
            _hull = hull;
            _facePool = hull.FacePool;
            _facePoolSize = 0;
            _facePoolCapacity = hull.FacePool.Length;
            _freeFaceIndices = new IndexBuffer();

            _emptyBufferStack = new SimpleList<IndexBuffer>();
            _deferredFaceStack = new SimpleList<DeferredFace>();
        }

        /// <summary>
        /// Return the face to the pool for later use.
        /// </summary>
        /// <param name="faceIndex">Index of the face.</param>
        public void DepositFace(int faceIndex)
        {
            ConvexFaceInternal face = _facePool[faceIndex];
            int[] af = face.AdjacentFaces;
            for (int i = 0; i < af.Length; i++)
            {
                af[i] = -1;
            }
            _freeFaceIndices.Push(faceIndex);
        }

        /// <summary>
        /// Reallocate the face pool, including the AffectedFaceFlags
        /// </summary>
        private void ReallocateFacePool()
        {
            ConvexFaceInternal[] newPool = new ConvexFaceInternal[2 * _facePoolCapacity];
            bool[] newTags = new bool[2 * _facePoolCapacity];
            Array.Copy(_facePool, newPool, _facePoolCapacity);
            Buffer.BlockCopy(_hull.AffectedFaceFlags, 0, newTags, 0, _facePoolCapacity * sizeof(bool));
            _facePoolCapacity = 2 * _facePoolCapacity;
            _hull.FacePool = newPool;
            _facePool = newPool;
            _hull.AffectedFaceFlags = newTags;
        }

        /// <summary>
        /// Create a new face and put it in the pool.
        /// </summary>
        /// <returns>System.Int32.</returns>
        private int CreateFace()
        {
            int index = _facePoolSize;
            ConvexFaceInternal face = new ConvexFaceInternal(_dimension, index, GetVertexBuffer());
            _facePoolSize++;
            if (_facePoolSize > _facePoolCapacity) ReallocateFacePool();
            _facePool[index] = face;
            return index;
        }

        /// <summary>
        /// Return index of an unused face or creates a new one.
        /// </summary>
        /// <returns>System.Int32.</returns>
        public int GetFace()
        {
            if (_freeFaceIndices.Count > 0) return _freeFaceIndices.Pop();
            return CreateFace();
        }

        /// <summary>
        /// Store a face connector in the "embedded" linked list.
        /// </summary>
        /// <param name="connector">The connector.</param>
        public void DepositConnector(FaceConnector connector)
        {
            if (_connectorStack == null)
            {
                connector.Next = null;
                _connectorStack = connector;
            }
            else
            {
                connector.Next = _connectorStack;
                _connectorStack = connector;
            }
        }

        /// <summary>
        /// Get an unused face connector. If none is available, create it.
        /// </summary>
        /// <returns>FaceConnector.</returns>
        public FaceConnector GetConnector()
        {
            if (_connectorStack == null) return new FaceConnector(_dimension);

            FaceConnector ret = _connectorStack;
            _connectorStack = _connectorStack.Next;
            ret.Next = null;
            return ret;
        }

        /// <summary>
        /// Deposit the index buffer.
        /// </summary>
        /// <param name="buffer">The buffer.</param>
        public void DepositVertexBuffer(IndexBuffer buffer)
        {
            buffer.Clear();
            _emptyBufferStack.Push(buffer);
        }

        /// <summary>
        /// Get a store index buffer or create a new instance.
        /// </summary>
        /// <returns>IndexBuffer.</returns>
        public IndexBuffer GetVertexBuffer()
        {
            return _emptyBufferStack.Count != 0 ? _emptyBufferStack.Pop() : new IndexBuffer();
        }

        /// <summary>
        /// Deposit the deferred face.
        /// </summary>
        /// <param name="face">The face.</param>
        public void DepositDeferredFace(DeferredFace face)
        {
            _deferredFaceStack.Push(face);
        }

        /// <summary>
        /// Get the deferred face.
        /// </summary>
        /// <returns>DeferredFace.</returns>
        public DeferredFace GetDeferredFace()
        {
            return _deferredFaceStack.Count != 0 ? _deferredFaceStack.Pop() : new DeferredFace();
        }
    }
}