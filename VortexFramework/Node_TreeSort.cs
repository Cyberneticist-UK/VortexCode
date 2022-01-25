using System;
using System.Collections.Generic;

namespace Vortex
{
    public class SortNode
    {
        public Node data;
        public SortNode Prev;
        public SortNode Next;
        public SortNode(Node data)
        {
            this.data = data;
        }
    }

    public class TreeSort
    {
        public SortNode Top = null;
        List<Node> Sorted = new List<Node>();

        /// <summary>
        /// This adds new nodes into the tree
        /// </summary>
        /// <param name="item">the new item to add</param>
        public void Push(Node item, Field SortField)
        {
            // Check to see if the top of the tree exists:
            if (Top == null)
                // Add it as the top of the tree, as it didnt!
                Top = new SortNode(item);
            else
                // Run through the recursive function:
                InsertIntoTree(Top, item, SortField);
        }

        public List<Node> SortedList()
        {
            // Reset the list:
            Sorted.Clear();
            Sorted = new List<Node>();
            OrderList(Top);
            return Sorted;
        }

        protected bool LessThan(Node item, Node test, Field SortField)
        {
            switch(SortField)
            {
                case Field.Content:
                    if (System.String.Compare(item.Content.ToString(), test.Content.ToString()) == -1)
                        return true;
                    else
                        return false;
                case Field.Created:
                    if (item.Created.Ticks < test.Created.Ticks)
                        return true;
                    else
                        return false;
                case Field.Date1:
                    if (item.Date1.Ticks < test.Date1.Ticks)
                        return true;
                    else
                        return false;
                case Field.Date2:
                    if (item.Date2.Ticks < test.Date2.Ticks)
                        return true;
                    else
                        return false;
                case Field.Date3:
                    if (item.Date3.Ticks < test.Date3.Ticks)
                        return true;
                    else
                        return false;
                case Field.ExtraInfo:
                    if (System.String.Compare(item.ExtraInfo.ToString(), test.ExtraInfo.ToString()) == -1)
                        return true;
                    else
                        return false;
                case Field.LastUpdated:
                    if (item.LastUpdated.Ticks < test.LastUpdated.Ticks)
                        return true;
                    else
                        return false;
                case Field.LinkCreated:
                    if (item.LinkCreated.Ticks < test.LinkCreated.Ticks)
                        return true;
                    else
                        return false;
                case Field.LinkLastUpdated:
                    if (item.LinkLastUpdated.Ticks < test.LinkLastUpdated.Ticks)
                        return true;
                    else
                        return false;
                case Field.SubContent:
                    if (System.String.Compare(item.SubContent.ToString(), test.SubContent.ToString()) == -1)
                        return true;
                    else
                        return false;
                case Field.Title:
                    if (System.String.Compare(item.Title.ToString(), test.Title.ToString()) == -1)
                        return true;
                    else
                        return false;
                case Field.Value1:
                    if ((Decimal)item.Value1 < (Decimal)test.Value1)
                        return true;
                    else
                        return false;
                case Field.Value2:
                    if ((Decimal)item.Value2 < (Decimal)test.Value2)
                        return true;
                    else
                        return false;
                case Field.Value3:
                    if ((Decimal)item.Value3 < (Decimal)test.Value3)
                        return true;
                    else
                        return false;
                case Field.ValueInfo:
                    if ((Decimal)item.ValueInfo < (Decimal)test.ValueInfo)
                        return true;
                    else
                        return false;
            }
            return true;
        }

        protected void InsertIntoTree(SortNode Branch, Node item, Field SortField)
        {
            if (LessThan(item, Branch.data, SortField))
            {
                if (Branch.Prev == null)
                    Branch.Prev = new SortNode(item);
                else
                    InsertIntoTree(Branch.Prev, item, SortField);
            }
            else
            {
                if (Branch.Next == null)
                    Branch.Next = new SortNode(item);
                else
                    InsertIntoTree(Branch.Next, item, SortField);
            }
        }

        protected void OrderList(SortNode item)
        {
            if (item.Prev != null)
            {
                OrderList(item.Prev);
            }
            Sorted.Add(item.data);
            if (item.Next != null)
            {
                OrderList(item.Next);
            }
        }

    }
}
