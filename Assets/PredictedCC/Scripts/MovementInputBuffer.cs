using System.Collections.Generic;
using UnityEngine;

namespace PredictedCharacterController
{
    public sealed class MovementInputBuffer
    {
        public int Count => inputsBuffer.Count;

        private readonly List<MovementReplicateData> inputsBuffer = new();
        private const ushort maxBufferSize = 1024;

        public void Write(MovementReplicateData replicate)
        {
            inputsBuffer.Add(replicate);

            if (inputsBuffer.Count > maxBufferSize)
                inputsBuffer.RemoveAt(0);
        }
        public void UpdateLastPosition(int index, Vector3 newPos)
        {
            var data = inputsBuffer[index];
            inputsBuffer[index] = new(data.Input, new(newPos, data.Predicted.VerticalVelocity, data.Predicted.Tick));
        }

        public void ClearOlder(int index) => inputsBuffer.RemoveRange(0, index + 1);

        public MovementReplicateData Get(int index) => inputsBuffer[index];
        public int GetIndex(uint tick)
        {
            for (var i = 0; i < inputsBuffer.Count; i++)
            {
                if (inputsBuffer[i].Predicted.Tick == tick)
                    return i;
            }

            return -1;
        }
    }
}