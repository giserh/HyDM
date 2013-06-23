#include "StdAfx.h"
#include "LogRecorder.h"
#include <stdio.h>
#include <io.h> 

/********************************************************************
��Ҫ���� : ������־�ļ�
������� :
�� �� ֵ :
//
//
�޸���־ :
*********************************************************************/
CLogRecorder::CLogRecorder(CString sLogFileName)
{
	if(_access(sLogFileName, 0) == -1)
	{
		m_bOpen = m_file.Open(sLogFileName, CFile::modeCreate | CFile::modeWrite | CFile::typeText);
	}
	else
	{
		m_bOpen = m_file.Open(sLogFileName, CFile::modeWrite | CFile::typeText);
		m_file.SeekToEnd();
	}
}
/********************************************************************
��Ҫ���� : д��־
������� :
�� �� ֵ :
//
//
�޸���־ :
*********************************************************************/
void CLogRecorder::WriteLog(CString sLog)
{
	if (m_bOpen && !sLog.IsEmpty())
	{
		CTime dtCur = CTime::GetCurrentTime();
		CString sLogTime = dtCur.Format("%Y/%m/%d %H:%M:%S");
		sLog = sLogTime + "-" + sLog+"\r\n";
		m_file.WriteString(sLog);
	}
}

//////////////////////////////////////////////////////////////////////////
//��Ҫ���� : �ر���־�ļ�
//������� :
//�� �� ֵ :
//�޸���־ :
//////////////////////////////////////////////////////////////////////////
void CLogRecorder::CloseFile()
{
	if (m_bOpen)
	{
		m_bOpen = FALSE;
		m_file.Close();
	}
	
}

//////////////////////////////////////////////////////////////////////////
//��Ҫ���� : �ر���־�ļ�
//������� :
//�� �� ֵ :
//�޸���־ :
//////////////////////////////////////////////////////////////////////////
CLogRecorder::~CLogRecorder(void)
{
	if (m_bOpen)
	{
		m_file.Close();
	}
}
