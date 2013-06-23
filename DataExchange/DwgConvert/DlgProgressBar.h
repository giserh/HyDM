#pragma once

#include "resource.h"
#include "TextProgressCtrl.h"
// CDlgProgressBar �Ի���

class CDlgProgressBar : public CDialog
{
    DECLARE_DYNAMIC(CDlgProgressBar)

public:
    CDlgProgressBar(CWnd* pParent = NULL);   // ��׼���캯��
    virtual ~CDlgProgressBar();

// �Ի�������
    enum { IDD = IDD_DLGPROGRESSBAR };

protected:
    virtual void DoDataExchange(CDataExchange* pDX);    // DDX/DDV ֧��

    DECLARE_MESSAGE_MAP()

public:

    BOOL CreateDlg(CWnd *pParentWnd = NULL);
    BOOL Show();
    BOOL Hide();
    BOOL Close();

public:
    // ������
    //CProgressCtrl m_progressBar;
    CTextProgressCtrl m_progressBar;
};
