using System;
using System.Collections.Generic;
using System.Text;

namespace Skyline.Core
{
    public class BuilderObject
    {
        //����8���ֶ�
        private int _builID;  // ������ ����
        private string _builName;  // ��������
      //  private string _builItemID;  // ������Ϣ��ID  ʹ��ʱע��ת��int
        private string _builHeight;  // ����߶� ʹ��ʱע��ת��double
        private string _builArea;  // ������� ʹ��ʱע��ת��double
        private string _builInfo;  // ����˵����Ϣ
        private string _builLongitude;  // ���徭�� ʹ��ʱע��ת��double
        private string _builLatitude;  // ����γ�� ʹ��ʱע��ת��double
        private string _builObjectid;
        /// <summary>
        /// ����γ�� ʹ��ʱע��ת��double
        /// </summary>
        public string BuilLatitude
        {
            get { return _builLatitude; }
            set { _builLatitude = value; }
        }
	

        /// <summary>
        /// ���徭�� ʹ��ʱע��ת��double
        /// </summary>
        public string BuilLongitude
        {
            get { return _builLongitude; }
            set { _builLongitude = value; }
        }
	

        /// <summary>
        /// ����˵����Ϣ
        /// </summary>
        public string BuilInfo
        {
            get { return _builInfo; }
            set { _builInfo = value; }
        }
	


        /// <summary>
        /// ������� ʹ��ʱע��ת��double
        /// </summary>
        public string BuilArea
        {
            get { return _builArea; }
            set { _builArea = value; }
        }
	

        /// <summary>
        /// ����߶� ʹ��ʱע��ת��double
        /// </summary>
        public string BuilHeight
        {
            get { return _builHeight; }
            set { _builHeight = value; }
        }
	

        /// <summary>
        /// ������Ϣ��ID  ʹ��ʱע��ת��int
        /// </summary>
        //public string BuilItemID
        //{
        //    get { return _builItemID; }
        //    set { _builItemID = value; }
        //}
	

        /// <summary>
        /// ��������
        /// </summary>
        public string BuilName
        {
            get { return _builName; }
            set { _builName = value; }
        }
	

        /// <summary>
        /// ������ ����
        /// </summary>
        public int BuilId
        {
            get { return _builID; }
            set { _builID = value; }
        }
        public string BuidObjectID
        {
            get { return _builObjectid; }
            set { _builObjectid = value; }
        }
    }
}



