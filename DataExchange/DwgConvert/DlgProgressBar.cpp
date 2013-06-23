// DlgProgressBar.cpp : ʵ���ļ�
//

#include "stdafx.h"
#include "DwgConvert.h"
#include "DlgProgressBar.h"


// CDlgProgressBar �Ի���

IMPLEMENT_DYNAMIC(CDlgProgressBar, CDialog)

CDlgProgressBar::CDlgProgressBar(CWnd* pParent /*=NULL*/)
        : CDialog(CDlgProgressBar::IDD, pParent)
{

}

CDlgProgressBar::~CDlgProgressBar()
{
}

void CDlgProgressBar::DoDataExchange(CDataExchange* pDX)
{
    CDialog::DoDataExchange(pDX);
    DDX_Control(pDX, IDC_PROGRESS_BAR_ALL, m_progressBar);
}


BEGIN_MESSAGE_MAP(CDlgProgressBar, CDialog)
END_MESSAGE_MAP()


// CDlgProgressBar ��Ϣ�������


BOOL CDlgProgressBar::CreateDlg(CWnd *pParentWnd)
{
    return this->Create(CDlgProgressBar::IDD, pParentWnd);
}


BOOL CDlgProgressBar::Show()
{
    return this->ShowWindow(SW_SHOW);
}

BOOL CDlgProgressBar::Hide()
{
    return this->ShowWindow(SW_HIDE);
}

BOOL CDlgProgressBar::Close()
{
    return DestroyWindow();
}