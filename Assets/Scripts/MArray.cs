using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.Text;

[System.Serializable]
public class MArray<T> : IEnumerable, ISerializable
{

    int[] dimensions;

    int length;

    public int[] Dimensions { get { return dimensions; } set { dimensions = value; /*TODO*/ } }

    T[] arr;

    public T[] OneDimensional { get { return arr; } }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    static int product(int[] arr)
    {
        if (arr == null || arr.Length <= 0)
            throw new Exception("Array is empty");
        int m = arr[0];
        for (int i = 1; i < arr.Length; i++)
            m *= arr[i];
        return m;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public MArray(params int[] dims)
    {
        dimensions = new int[dims.Length];
        Array.Copy(dims, dimensions, dims.Length);
        length = product(dimensions);
        arr = new T[length];
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public int getIndex(params int[] i)
    {
        /*int d = dimensions[i.Length - 1];
        int ti = i[i.Length - 1];
        for (int j = i.Length - 2; j >= 0; j--)
        {
            ti += i[j] * d;
            d *= dimensions[j];
        }
        return ti;*/
        
        //coords -> index => inx = x0 * d0 + x1 * (d0 * d1) + x2 * (d0 * d1 * d2) + ... + xn * (d0 * d1 * ... * dn), d0 = 1
        int inx = 0;
        int product = 1;
        for(int j = 0; j < i.Length; j++)
        {
            inx += product * i[j];
            product *= dimensions[j];
        }
        return inx;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public int[] getCoords(int index)
    {
        /*int[] r = new int[dimensions.Length];
        int d0 = dimensions[0];
        r[0] = index % d0;
        for(int i = 1; i < r.Length; i++)
        {
            r[i] = index / d0;
            d0 *= dimensions[0];
        }
        return r;*/

        //index -> coords => (index % d0, index / d0 % d1, index / (d0*d1) % d2, ..., index / (d0*d1*...*dn-1) % dn)

        int[] r = new int[dimensions.Length];

        int product = 1;

        for(int j = 0; j < dimensions.Length; j++)
        {
            r[j] = index / product % dimensions[j];
            product *= dimensions[j];
        }

        return r;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void getCoordsNonAlloc(int index, ref int[] coords)
    {
        if (coords.Length < dimensions.Length)
            throw new Exception("Given array cannot store the coordinates of given MArray.");
        /*int d0 = dimensions[0];
        coords[0] = index % d0;
        for (int i = 1; i < coords.Length; i++)
        {
            coords[i] = index / d0;
            d0 *= dimensions[0];
        }*/

        int product = 1;

        for (int j = 0; j < dimensions.Length; j++)
        {
            coords[j] = index / product % dimensions[j];
            product *= dimensions[j];
        }

    }

    public T this[params int[] i]
    {
        get { return arr[getIndex(i)]; }
        set { arr[getIndex(i)] = value; }
    }

    public int Length { get { return length; } }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public int GetLength(int dimension)
    {
        return dimensions[dimension];
    }

    void forEach(Action<T> action)
    {
        for (int i = 0; i < length; i++)
        {
            action(arr[i]);
        }
    }


    #region IENUMERATOR_IMPL

    public IEnumerator GetEnumerator()
    {
        return new MArrayEnumerator(this);
    }


    public class MArrayEnumerator : IEnumerator<T>
    {

        public MArray<T> arr;

        int position = -1;

        public MArrayEnumerator(MArray<T> arr)
        {
            this.arr = arr;
        }

        public T Current { get { return arr.OneDimensional[position]; } set { arr.OneDimensional[position] = value; } }

        object IEnumerator.Current { get { return Current; } }

        public void Dispose()
        {
            
        }

        public bool MoveNext()
        {
            position++;
            return position < arr.Length;
        }

        public void Reset()
        {
            position = -1;
        }
    }

    #endregion

    public void GetObjectData(SerializationInfo info, StreamingContext context)
    {
        info.AddValue("dimensions", dimensions);
        info.AddValue("length", length);
        info.AddValue("arr", arr);
    }

    public override string ToString()
    {
        StringBuilder sb = new StringBuilder(1024);

        sb.Append("{");

        for(int i = 0; i < OneDimensional.Length - 1; i++)
        {
            sb.Append(OneDimensional[i].ToString());
            sb.Append(", ");
        }

        if (OneDimensional.Length > 0)
            sb.Append(OneDimensional[OneDimensional.Length - 1]);

        sb.Append("}");

        return sb.ToString();
    }

}
