using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using System.Text;

namespace InstrumentCommon
{
    /// <summary>
    /// 仪器操作命令类
    /// </summary>
    internal class InstrumentCommand
    {
        #region-----------------------------------------构造函数-----------------------------
        /// <summary>
        /// 构造函数
        /// </summary>
        public InstrumentCommand(string ModelID)
        {
            OpenResourceManager(ModelID);
            OpenSession();
        }

        #endregion----------------------------------------------------------------------------
        #region-------------------------------------------------公有方法-----------------------------
        /// <summary>
        /// 发送命令
        /// </summary>
        /// <param name="sendCommand"></param>
        /// <returns></returns>
        public int SendCommand(string sendCommand)
        {
            _byteArrayCommand = System.Text.Encoding.Default.GetBytes(sendCommand);
            _viError = visa32.viWrite(_viSession, _byteArrayCommand, _byteArrayCommand.Length, out _writeCnt);
            if (_viError != visa32.VI_SUCCESS)
            {
                return _viError;
            }
            return visa32.VI_SUCCESS;
        }

        /// <summary>
        /// 接收结果
        /// </summary>
        /// <param name="reviceResult"></param>
        /// <returns></returns>
        public int ReviceResult(ref StringBuilder reviceResult)
        {
            _viError = visa32.viScanf(_viSession, "%100t", reviceResult);
            if (_viError != visa32.VI_SUCCESS)
            {
                return _viError;
            }
            return visa32.VI_SUCCESS;
        }

        #endregion-------------------------------------------------------------------------

        #region-------------------------------------私有字段--------------------------------
        /// <summary>
        /// 缺省资源通道标识数
        /// </summary>
        private int _viDefaultRM;

        /// <summary>
        /// 特定资源通道标识数
        /// </summary>
        private int _viSession;

        /// <summary>
        /// VISA错误标识数
        /// </summary>
        private int _viError;

        /// <summary>
        /// 资源查询标识数
        /// </summary>
        private int _viFindRsrc;

        /// <summary>
        /// 参数返回个数
        /// </summary>
        private int _retCnt;

        /// <summary>
        /// 设备USB存放地址
        /// </summary>
        private string _instrAddress;

        /// <summary>
        /// _instrAddress的属性参量
        /// </summary>
        public string InstrAddress
        {
            get { return _instrAddress; }
        }

        /// <summary>
        /// 设备USB存放地址
        /// </summary>
        private StringBuilder InstrUSBAddress = new StringBuilder(100);

        /// <summary>
        /// 转换后的命令数组
        /// </summary>
        private byte[] _byteArrayCommand;

        private int _writeCnt;

        #endregion----------------------------------------------------------------------
        #region---------------------------------------私有方法-------------------------------
        /// <summary>
        /// 打开缺省资源管理器对话通道
        /// </summary>
        private void OpenResourceManager(string ModelID)
        {
            if (_viSession != 0)
            {
                visa32.viClose(_viSession);
            }
            if (_viDefaultRM != 0)
            {
                visa32.viClose(_viDefaultRM);
            }
            bool USBID = true;
            _viError = visa32.viOpenDefaultRM(out _viDefaultRM);
            if (_viError < 0)
            {
                throw new Exception("初始化失败，未查找到硬件！");
            }

            _viError = visa32.viFindRsrc(_viDefaultRM, "?*INSTR", out _viFindRsrc, out _retCnt, InstrUSBAddress);
            if (_viError < 0)
            {
                throw new Exception("初始化失败，未查找到硬件！");
            }
            while (!InstrUSBAddress.ToString().Contains(ModelID))
            {
                USBID = false;
                if (USBID == false)
                {
                    _viError = visa32.viFindNext(_viFindRsrc, InstrUSBAddress);
                    if (_viError < 0)
                    {
                        throw new Exception("初始化失败，请检查板卡编号或线缆连接！");
                    }
                }
            }

            _instrAddress = InstrUSBAddress.ToString();

        }

        /// <summary>
        /// 打开特定资源对话通道
        /// </summary>
        private void OpenSession()
        {
            //设备打开
            _viError = visa32.viOpen(_viDefaultRM, _instrAddress, visa32.VI_NO_LOCK, 5000, out _viSession);
            if (_viError < visa32.VI_SUCCESS)
            {
                throw new Exception("初始化失败，请检查板卡编号或线缆连接！");
            }
            _viError = visa32.viClear(_viSession);
            // 设备通讯
            StringBuilder strResult = new StringBuilder(1000);
            string Idresult;
            string command = "*IDN?\n";

            _viError = visa32.viPrintf(_viSession, command);
            Thread.Sleep(100);
            //_viError = visa32.viScanf(_viSession, "%100t", strResult);
            //Idresult = strResult.ToString();
            //if (!Idresult.Contains("RIGOL"))
            //{
            //    throw new Exception("设备通讯失败，请检查板卡编号或线缆连接！");
            //}

            // 设备复位
            command = "*RST";
            _byteArrayCommand = System.Text.Encoding.Default.GetBytes(command);
            _viError = visa32.viWrite(_viSession, _byteArrayCommand, _byteArrayCommand.Length, out _writeCnt);
            if (_viError != visa32.VI_SUCCESS)
            {
                throw new Exception("设备复位失败！");
            }
        }
        #endregion-------------------------------------------------------------------------------

    }

    /// <summary>
    /// 错误代码的定义
    /// </summary>
    internal static class JYErrorCode
    {
        /// <summary>
        /// 无错误
        /// </summary>
        public static int NoError = 0;

        /// <summary>
        /// 超时
        /// </summary>
        public static int TimeOut = -10001;

        /// <summary>
        /// 参数错误
        /// </summary>
        public static int ErrorParam = -10002;

        /// <summary>
        /// 调用顺序不正确
        /// </summary>
        public static int IncorrectCallOrder = -10003;

        /// <summary>
        /// 当前配置不能调用该方法
        /// </summary>
        public static int CannotCall = -10004;

        /// <summary>
        /// 用户缓冲区错误
        /// </summary>
        public static int UserBufferError = -10005;

        /// <summary>
        /// 缓冲区溢出
        /// </summary>
        public static int BufferOverflow = -10006;

        /// <summary>
        /// 缓冲区下溢出
        /// </summary>
        public static int BufferDownflow = -10007;


        /// <summary>
        /// 频率超出范围
        /// </summary>
        public static int FEQOver = -1001;

        /// <summary>
        /// CH1幅值超出范围
        /// </summary>
        public static int VppOver1 = -1002;

        /// <summary>
        /// CH2幅值超出范围
        /// </summary>
        public static int VppOver2 = -1003;

        /// <summary>
        ///占空比超出范围
        /// </summary>
        public static int DcyOver = -1004;

        /// <summary>
        ///锯齿波对称比超出范围 
        /// </summary>
        public static int SymOver = -1005;

        /// <summary>
        /// 通道错误
        /// </summary>
        public static int Errorch = -1006;

        /// <summary>
        /// 相移错误
        /// </summary>
        public static int Errorpha = -1007;

        /// <summary>
        /// 偏置电压错误
        /// </summary>
        public static int Erroroffs = -1008;

        /// <summary>
        /// 频率输入错误
        /// </summary>
        public static int Errorfreq = -1009;

        /// <summary>
        /// 占空比输入错误
        /// </summary>
        public static int Wrongdcy = -1010;

        /// <summary>
        /// 脉冲波错误设置
        /// </summary>
        public static int WrongPul = -1011;

        /// <summary>
        /// 耦合相位错误
        /// </summary>
        public static int Wrongcoupha = -1012;

        /// <summary>
        /// 耦合频率偏差错误
        /// </summary>
        public static int Wrongcoufeq = -1013;

        /// <summary>
        /// 脉冲相关范围错误
        /// </summary>
        public static int PulseError = -1014;

        /// <summary>
        /// 调制频率超限
        /// </summary>
        public static int AMInternalFreqOver = -1015;

        /// <summary>
        /// 调制深度错误
        /// </summary>
        public static int AMDerthOver = -1016;

        /// <summary>
        /// 频偏超限
        /// </summary>
        public static int DeviationFreqOver = -1017;

        /// <summary>
        /// 跳频超限
        /// </summary>
        public static int FSKFreqOver = -1018;

        /// <summary>
        /// 速率超限
        /// </summary>
        public static int FSKRateOver = -1019;

        /// <summary>
        /// 扫频时间超限
        /// </summary>
        public static int SweepTimeOver = -1020;

        /// <summary>
        /// 脉冲串周期超限
        /// </summary>
        public static int BrustTimeOver = -1021;

        /// <summary>
        /// 脉冲串循环超限
        /// </summary>
        public static int BrustCyclesOver = -1022;

        /// <summary>
        /// 脉冲串相移超限
        /// </summary>
        public static int BurstPhaseOver = -1023;

        /// <summary>
        /// 频率耦合计电平超限
        /// </summary>
        public static int CountLevelOver = -1024;
    }
}

