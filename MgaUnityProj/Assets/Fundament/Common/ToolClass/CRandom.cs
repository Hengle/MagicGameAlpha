using System.Collections.Generic;
using System.Collections;
using System.Linq;

public class CRandom
{
    /// <summary>
    /// 种子
    /// </summary>
    static public uint seed = 0;

    private const uint seed1 = 4096;
    private const uint seed2 = 150889;
    private const uint maxnumber = 714025;

    /// <summary>
    /// 返回 0 - 1 之间的一个小数
    /// </summary>
    /// <returns></returns>
    public static float NextF()
    {
        seed = seed1 * seed + seed2;
        seed = seed % maxnumber;
        return seed/(float) maxnumber;
    }

    /// <summary>
    /// 返回 0 - maxnumber 之间的一个整数
    /// </summary>
    /// <returns></returns>
    private static int NextI()
    {
        seed = seed1 * seed + seed2;
        seed = seed % maxnumber;
        return (int)seed;
    }

    //For replay to use a uniq random
    /// <summary>
    /// 返回f1 到 f2 之间的一个随机小数
    /// </summary>
    /// <param name="f1">下限</param>
    /// <param name="f2">上限</param>
    /// <param name="iseed">种子</param>
    /// <returns></returns>
    static public float Range(float f1, float f2, uint iseed)
    {
        seed = iseed;
        return NextF()*(f2 - f1) + f1;
    }

    /// <summary>
    /// 返回f1 到 f2 - 1 之间的一个随机整数
    /// </summary>
    /// <param name="f1">下限</param>
    /// <param name="f2">上限 + 1</param>
    /// <param name="iseed">种子</param>
    /// <returns></returns>
    static public int Range(int f1, int f2, uint iseed)
    {
        seed = iseed;
        return NextI()%(f2 - f1) + f1;
    }

    /// <summary>
    /// 返回f1 到 f2 之间的一个随机小数
    /// </summary>
    /// <param name="f1">下限</param>
    /// <param name="f2">上限</param>
    /// <returns></returns>
    static public float Range(float f1, float f2)
    {
        return NextF() * (f2 - f1) + f1;
    }

    /// <summary>
    /// 返回f1 到 f2 之间的一个随机整数
    /// </summary>
    /// <param name="f1">下限</param>
    /// <param name="f2">上限</param>
    /// <returns></returns>
    static public int Range(int f1, int f2)
    {
        return NextI() % (f2 - f1) + f1;
    }

    //For safely Hmac-Sha1 a string
    private class SOrder
    {
        public int theOld;
        public int theNew;
        public int orderValue;
    }

    static private int Sort1(SOrder o1, SOrder o2)
    {
        return o1.orderValue >= o2.orderValue ? 1 : -1;
    }

    public static byte[] DisOrder(byte[] bytes, int iSeed)
    {
        seed = (uint)(iSeed * iSeed * iSeed);

        SOrder[] tmpOrder = new SOrder[bytes.Length];

        for (int i = 0; i < bytes.Length; ++i)
        {
            tmpOrder[i] = new SOrder();
            tmpOrder[i].theOld = i;
            tmpOrder[i].orderValue = NextI();
        }
        List <SOrder> tmpList = tmpOrder.OfType<SOrder>().ToList();
		tmpList.Sort(Sort1);
		
        for (int i = 0; i < bytes.Length; ++i)
        {
            tmpList[i].theNew = i;
        }

        byte[] ret = new byte[bytes.Length];
        for (int i = 0; i < bytes.Length; ++i)
        {
            ret[tmpList[i].theNew] = bytes[tmpList[i].theOld];
        }

        return ret;
    }

    public static byte[] ReOrder(byte[] bytes, int iSeed)
    {
        seed = (uint)(iSeed * iSeed * iSeed);

        SOrder[] tmpOrder = new SOrder[bytes.Length];

        for (int i = 0; i < bytes.Length; ++i)
        {
            tmpOrder[i] = new SOrder();
            tmpOrder[i].theOld = i;
            tmpOrder[i].orderValue = NextI();
        }
        List <SOrder> tmpList = tmpOrder.OfType<SOrder>().ToList();
		tmpList.Sort(Sort1);
		
        for (int i = 0; i < bytes.Length; ++i)
        {
            tmpList[i].theNew = i;
        }

        byte[] ret = new byte[bytes.Length];
        for (int i = 0; i < bytes.Length; ++i)
        {
            ret[tmpList[i].theOld] = bytes[tmpList[i].theNew];
        }

        return ret;        
    }

}
