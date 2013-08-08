//*****************************************
//�����ã���ȡ��ǰ��ǰ�ӽ�λ����Ϣ
//�����ߣ�����
//���ڣ�2009-5-13
//�汾��1.0
//˵��������ҪTerraExplorerClass����  ͨ��Program.TE����
//*****************************************

using System;
using System.Collections.Generic;
using System.Text;
using TerraExplorerX;


namespace Skyline.Core.UI
{
    /// <summary>
    /// ��ȡ��ǰ�ӽ�λ����Ϣ
    /// </summary>
    public class GetPositionInfo
    {
        //8������
        private double _longitude;// ��ǰ�ӽǾ���
        private double _latitude;// ��ǰ�ӽ�γ��
        private double _height;// ��ǰ�ӽǸ߶�
        private double _yaw;// ��ǰ�ӽǷ�λ
        private double _pitch;//  ��ǰ�ӽ�������б�Ƕ�
        private double _roll;// ��ǰ�ӽ�������б�Ƕ�
        private double _cameraDeltaYaw;// ��������Ǽܷ�λ
        private double _cameraDeltaPitch;// ��������Ǽ�������б�Ƕ�

        #region -------------------------����-------------------------

        /// <summary>
        /// ��������Ǽ�������б�Ƕ�
        /// </summary>
        public double CameraDeltaPitch
        {
            get { return _cameraDeltaPitch; }
            set { _cameraDeltaPitch = value; }
        }


        /// <summary>
        /// ��������Ǽܷ�λ
        /// </summary>
        public double CameraDeltaYaw
        {
            get { return _cameraDeltaYaw; }
            set { _cameraDeltaYaw = value; }
        }


        /// <summary>
        /// ��ǰ�ӽ�������б�Ƕ�
        /// </summary>
        public double Roll
        {
            get { return _roll; }
            set { _roll = value; }
        }


        /// <summary>
        ///  ��ǰ�ӽ�������б�Ƕ�
        /// </summary>
        public double Pitch
        {
            get { return _pitch; }
            set { _pitch = value; }
        }


        /// <summary>
        /// ��ǰ�ӽǷ�λ
        /// </summary>
        public double Yaw
        {
            get { return _yaw; }
            set { _yaw = value; }
        }

        /// <summary>
        /// ��ǰ�ӽǸ߶�
        /// </summary>
        public double Height
        {
            get { return _height; }
            set { _height = value; }
        }


        /// <summary>
        /// ��ǰ�ӽ�γ��
        /// </summary>
        public double Latitude
        {
            get { return _latitude; }
            set { _latitude = value; }
        }

        /// <summary>
        /// ��ǰ�ӽǾ���
        /// </summary>
        public double Longitude
        {
            get { return _longitude; }
            set { _longitude = value; }
        }

        #endregion -------------------------����-------------------------

        /// <summary>
        /// ���췽��
        /// </summary>
        public GetPositionInfo()
        {
           
        }


        /// <summary>
        /// ��ȡ��ǰ��ǰ�ӽ�λ����Ϣ
        /// ������ӽ�λ�á�����ô����ģ�
        /// </summary>
        public void GetPosition()
        {
            try
            {
                object longitude, latitude, height, yaw, pitch, roll, careraDeltaYaw, cameraDeltaPitch;
                Program.TE.IPlane5_GetPosition(out longitude, out latitude, out height, out yaw, out pitch, out roll, out careraDeltaYaw, out cameraDeltaPitch);
                this.Longitude = Convert.ToDouble(longitude.ToString());
                this.Latitude = Convert.ToDouble(latitude.ToString());
                this.Height = Convert.ToDouble(height.ToString());
                this.Yaw = Convert.ToDouble(yaw.ToString());
                this.Pitch = Convert.ToDouble(pitch.ToString());
                this.Roll = Convert.ToDouble(roll.ToString());
                this.CameraDeltaYaw = Convert.ToDouble(careraDeltaYaw.ToString());
                this.CameraDeltaPitch = Convert.ToDouble(cameraDeltaPitch.ToString());
            }
            catch (Exception)
            {
                
               
            }
           
        }

    }
}
