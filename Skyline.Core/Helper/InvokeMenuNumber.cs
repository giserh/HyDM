//*****************************************
//�����ã����ò˵��ż��� ����skyline��invoke�˵�����
//�����ߣ�����
//���ڣ�2009-5-13
//�汾��1.0
//˵��������ҪTerraExplorerClass����  ͨ��Program.TE����
//*****************************************

using System;
using System.Collections.Generic;
using System.Text;
using TerraExplorerX;

namespace Skyline.Core
{
    public enum InvokeNumber
    {
        /// <summary>
        /// ��ʾ���� ---������
        /// </summary>
        House = 32771,
        /// <summary>
        /// ��ʾ���� ---���ֵ�
        /// </summary>
        Street = 32772,
        /// <summary>
        /// ��ʾ���� ---������
        /// </summary>
        City = 32773,
        /// <summary>
        /// ��ʾ���� ---������
        /// </summary>
        State = 32774,
        /// <summary>
        /// ��ʾ���� ---������
        /// </summary>
        Globe = 32775,
        /// <summary>
        /// ����ִ����һ�� ִ�еĶ�������
        /// </summary>
        Play = 1011,
        /// <summary>
        /// ����ֹͣ ִ�еĶ���
        /// </summary>
        Stop = 1010,
        /// <summary>
        /// ����ģʽ ����Ļ�е�Ϊ�Ļ�����ת
        /// </summary>
        FlyAround = 34026,
        /// <summary>
        /// ָ��ģʽ ����ʹmpt��ָ�򱱷�
        /// </summary>
        FaceNorth = 7008,
        /// <summary>
        /// ��קģʽ ��Ĭ�ϵģ�
        /// </summary>
        Drag = 1022,
        /// <summary>
        /// ���ģʽ
        /// </summary>
        Slide = 1021,
        /// <summary>
        /// ��תģʽ
        /// </summary>
        TurnAndTilt = 1023,
        /// <summary>
        /// ���� �����ǰ�ӽ���ʾ��ͼƬ
        /// </summary>
        Snapshot = 32783,
        /// <summary>
        /// ˮƽ������� ����ˮƽ����
        /// </summary>
        Horizontal = 33326,
        /// <summary>
        /// �ռ�������  ����ˮƽ����
        /// </summary>
        Aerial = 33327,
        /// <summary>
        /// ��ֱ�������
        /// </summary>
        Vertical = 33330,
        /// <summary>
        /// ���浱ǰ�ļ�
        /// </summary>
        Save = 57603,
        /// <summary>
        /// �������
        /// </summary>
        Area = 33350,
        /// <summary>
        /// ����ģʽ
        /// </summary>
        Underground = 33372,
        /// <summary>
        /// ��ƽ��ģʽ(ע��ʹ��ǰҪ�������)
        /// </summary>
        BehindObject = 34200,
        /// <summary>
        /// ����
        /// </summary>
        Copy=32817,
        /// <summary>
        /// ճ��
        /// </summary>
        Paste=32819,
        /// <summary>
        /// ѡ��
        /// </summary>
        SelectObject = 33432,



    }
    }
