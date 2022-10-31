//==============================================================================
// Filename: Heap.cs
// Author: Aaron Thompson
// Date Created: 10/9/2021
// Last Updated: 10/18/2021
//
// Description:
//==============================================================================
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//------------------------------------------------------------------------------
public class Heap<T> where T : IHeapElement<T>{
// VARIABLES
//------------------------------------------------------------------------------
    private T[] data;
    private int count;
    public int Count { get { return count; } }

// PUBLIC FUNCTIONS
//------------------------------------------------------------------------------
    public Heap(int maxHeapSize){
        data = new T[maxHeapSize];
    }

    public void Push(T e) {
        e.HeapIndex = count;
        data[count] = e;
        SortUp(e);
        count++;
    }

    public T Pop() {
        T top = data[0];
        data[0] = data[count - 1];
        data[0].HeapIndex = 0;
        SortDown(data[0]);
        count--;
        return top;
    }

    public void UpdateItem(T e) {
        SortUp(e);
    }

    public bool Contains(T e) {
        return Equals(data[e.HeapIndex], e);
    }

// UTILITY FUNCTIONS
//------------------------------------------------------------------------------

    private void SortUp(T e) {
        int parentIndex = (e.HeapIndex - 1) / 2;

        while(true) {
            T parent = data[parentIndex];
            if(e.CompareTo(parent) > 0) {
                Swap(e, parent);
            } else {
                break;
            }

            parentIndex = (e.HeapIndex - 1) / 2;
        }
    }

    private void SortDown(T e) {
        while(true) {
            //Left/Right Children
            int left = 2 * e.HeapIndex + 1;
            int right = 2 * e.HeapIndex + 2;
            int swapIndex = 0;

            if(left < count) {
                swapIndex = left;

                if(right < count) {
                    if (data[left].CompareTo(data[right]) < 0) {
                        swapIndex = right;
                    }
                }

                if(e.CompareTo(data[swapIndex]) < 0){
                    Swap(e, data[swapIndex]);
                } else {
                    return;
                }
            } else {
                return;
            }
        }
    }

    private void Swap(T a, T b) {
        data[a.HeapIndex] = b;
        data[b.HeapIndex] = a;
        int temp = a.HeapIndex;
        a.HeapIndex = b.HeapIndex;
        b.HeapIndex = temp;
    }
}

// INTERFACE
//------------------------------------------------------------------------------
public interface IHeapElement<T> : System.IComparable<T> {
    int HeapIndex
    {
        get;
        set;
    }
}
//==============================================================================
//==============================================================================
