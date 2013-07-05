using System.Collections.Generic;
using DevExpress.XtraBars;
using ESRI.ArcGIS.Display;
using Common.UI;
using Hy.Check.Task;
using Hy.Check.Engine;
using Hy.Check.Utility;

namespace Hy.Check.Command
{
    /// <summary>
    /// �洢�������е�ȫ�ֱ���
    /// </summary>
    public class CheckApplication
    {

        private static XProgress progressBar;

        /// <summary>
        /// ��ȡȫ�ֵĽ�����.
        /// </summary>
        /// <value>The progress bar.</value>
        public static XProgress ProgressBar
        {
            get
            {
                if (progressBar == null)
                    progressBar = new XProgress();
                return progressBar;
            }
        }

        private static XGifProgress gifProgressBar;


        /// <summary>
        /// ��ȡȫ�ֵĶ���������.
        /// </summary>
        /// <value>The GIF progress.</value>
        public static XGifProgress GifProgress
        {
            get 
            {
                if (gifProgressBar == null)
                    gifProgressBar = new XGifProgress();
                return gifProgressBar;
            }
        }

        /// <summary>
        /// ��ǰ����
        /// </summary>
        public static Hy.Check.Task.Task CurrentTask{get;set;}

        /// <summary>
        /// ��ǰ�������Ƿ�Ԥ�죬����ǣ���Ϊtrue
        /// </summary>
        public static bool IsPreCheck;
 
        /// <summary>
        /// ��ǰ�Ƿ���һ��ť����ʹ���У�����ǣ���Ϊtrue
        /// </summary>
        public static bool IsInUse = false;

        public static Hy.Check.UI.UC.UCMapControl m_UCDataMap ;
        
        /// <summary>
        /// ����ı�ʱ������һ��ϵͳֻ��������һ������
        /// </summary>
        /// <param name="NewTask"></param>
        public static void TaskChanged(Hy.Check.Task.Task NewTask)
        {
            //�Ƚ���ǰ�ʼ�����е�������գ�Ȼ���ټ��������ʼ�����
            //m_UCDataMap.SetTask(null);
            m_UCDataMap.SetTask(NewTask);
        }

        private static TemplateRules m_CurrentTemplateRules = null;

        /// <summary>
        /// ���ݷ���id��ʼ����ǰ��������Ĺ�����
        /// </summary>
        /// <returns></returns>
        public  static TemplateRules InitCurrentTemplateRules()
        {
            if (m_CurrentTemplateRules == null)
            {
                if (CurrentTask == null)
                    return null;

                if (string.IsNullOrEmpty(CurrentTask.SchemaID)) return null;

                m_CurrentTemplateRules = new TemplateRules(CurrentTask.SchemaID);
            }
            return m_CurrentTemplateRules;
        }

        /// <summary>
        /// �������������������ʾ�Ƿ����·����Ŀ¼�µ�����Դ
        /// </summary>
        public static bool BolIgnoreRootFile;

        /// <summary>
        /// �����������������ָ������Դ���ڵ����·��
        /// </summary>
        public static string RelationalPath;
    }
}