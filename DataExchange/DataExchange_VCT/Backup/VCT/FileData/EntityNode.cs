///////////////////////////////////////////////////////////
//  EntityNode.cs
//  Implementation of the Class EntityNode
//  Generated by Enterprise Architect
//  Created on:      08-����-2011 13:45:30
//  Original author: Administrator
///////////////////////////////////////////////////////////


using System.Collections.Generic;
using System;

namespace DIST.DGP.DataExchange.VCT.FileData {
	/// <summary>
	/// VCTʵ��ڵ���
	/// </summary>
    [Serializable]
	public class EntityNode {

        /// <summary>
        /// �ڼ����е�Ψһ��ʶ��
        /// </summary>
        public int IndexID;
        //{
        //    get
        //    {
        //        return m_nIndex;
        //    }
        //    set
        //    {
        //        m_nIndex = value;
        //    }
        //}
        //private int m_nIndex;

		/// <summary>
		/// ��ʶ��
		/// </summary>
        public int EntityID;
        //{
        //    get
        //    {
        //        return m_nEntityID;
        //    }
        //    set
        //    {
        //        m_nEntityID = value;
        //    }
        //}
        //private int m_nEntityID;

		/// <summary>
		/// Ҫ�ش���
		/// </summary>
        public string FeatureCode;
        //{
        //    get
        //    {
        //        return m_strFeatureCode;
        //    }
        //    set
        //    {
        //        m_strFeatureCode = value;
        //    }
        //}
        //private string m_strFeatureCode;


		/// <summary>
		/// ͼ�α��ֱ���
		/// </summary>
        public string Representation;
        //{
        //    get
        //    {
        //        return m_strRepresentation;
        //    }
        //    set
        //    {
        //        m_strRepresentation = value;
        //    }
        //}
        //private string m_strRepresentation;

		public EntityNode()
        {
            //m_nEntityID = -1;
            //m_strFeatureCode = "";
            //m_strRepresentation = "";
            EntityID = -1;
            FeatureCode = "";
            Representation = "Unknown";
        }

        //~EntityNode(){

        //}

        //public virtual void Dispose(){

        //}

        /// <summary>
        /// ���VCT�ڵ�
        /// </summary>
        public override string ToString()
        {
            string strNode = this.EntityID.ToString();
            strNode += "\r\n" + this.FeatureCode;
            strNode += "\r\n" + this.Representation;
            return strNode;
        }
	}//end EntityNode

    /// <summary>
    /// ʵ��ڵ㼯����
    /// </summary>
    public class EntityNodes : List<EntityNode>
    {
        /// <summary>
        /// ʵ���ʶ����������֮���ӳ��
        /// </summary>
        Dictionary<int, int> arrEntityID = new Dictionary<int, int>();
        //private int[] arrEntityID = new int[100000000];//2147483647

        public EntityNodes()
        {
            Init();
        }

        /// <summary>
        /// ��ʼ������
        /// </summary>
        private void Init()
        {
            arrEntityID.Clear();
            //for (int i = 0; i < arrEntityID.Length; i++)
            //{
            //    arrEntityID[i] = -1;
            //}
        }

        /// <summary>
        /// ����ʵ��ڵ�
        /// </summary>
        public new void Add(EntityNode item)
        {
            base.Add(item);
            //arrEntityID[item.EntityID] = this.Count - 1;
            arrEntityID[item.EntityID] = this.Count - 1;
        }

        ///// <summary>
        ///// ����ʵ��ڵ㼯��
        ///// </summary>
        //public new void AddRange(IEnumerable<EntityNode> collection)
        //{
        //    IEnumerator<EntityNode> pIEnumerable = null;

        //    while ((pIEnumerable = collection.GetEnumerator()) != null)
        //    {
        //        this.Add(pIEnumerable.Current);
        //    }
        //}

        /// <summary>
        /// ����ʵ���ʶ���ȡʵ��ڵ�
        /// </summary>
        public EntityNode GetItemByEntityID(int nEntityID)
        {
            int nIndex = arrEntityID[nEntityID];
            if (nIndex != -1)
                return this[nIndex];
            return null;
        }

        /// <summary>
        /// ��������
        /// </summary>
        public new void Sort()
        {
            //������
            base.Sort();
            //�����ԭ������
            Init();
            //����ؽ�����
            for (int i = 0; i < this.Count; i++)
            {
                arrEntityID[this[i].EntityID] = i;
            }
        }
    }//end EntityNodes
}//end namespace FileData