/*
*
* Project : WURFL Microservice .NET Client API
*
* Author(s):  Andrea Castello
*
* Date: October 3 2019
*
* Copyright(c) ScientiaMobile, Inc.
* http://www.scientiamobile.com
*
*/
using System;
using System.Collections.Generic;

namespace NUnit_Test
{
    class TestData
    {
        static String[] UserAgents = new String[]
        {
        "5.0 (Macintosh; Intel Mac OS X 10_11_6) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/55.0.2883.95 Safari/537.36 ASXACT12779",
        "97718A_ABROAD/V1 Linux/3.4.5 Android/4.2.2 Release/05.02.2013 Browser/AppleWebKit534.30 Mobile Safari/534.30 MBBMS/2.2;",
        "A1034/1.0 Browser/Obigo/Q03C Profile",
        "A10-70F/Mozilla/5.0 (Linux; U; Android 4.4.2; de-ch; DG800 Build/KOT49H) AppleWebKit/534.30 (KHTML, like Gecko) Version/4.0 UCBrowser/10.7.0.636 U3/0.8.0 Mobile Safari/534.30",
        "A117_MULB Linux/3.0.13 Android/4.0.4 Release/01.24.2013 Browser/AppleWebKit534.30 Profile/MIDP-2.0 Configuration/CLDC-1.1 Mobile Safari/534.30 Android 4.0.1;",
        "A14Plus/V1 Linux/3.4.5 Android/4.2.2 Release/03.26.2013 Browser/AppleWebKit534.30 Mobile Safari/534.30 MBBMS/2.2;",
        "A150/A150 Linux/3.4.5+ Android/4.2.2 Release/A150 Browser/AppleWebKit534.30 Profile/ Configuration/ Mobile Safari/534.30",
        "A1 Sitemap Generator/1.8.8 (+http://www.microsystools.com/products/sitemap-generator/) miggibot",
        "ALCATEL_one_touch_803A/1.0 Profile/MIDP-2.0 Configuration/CLDC-1.1 ObigoInternetBrowser/Q05A",
        "ALCATEL_one_touch_810D/1.0 Profile/MIDP-2.0 Configuration/CLDC-1.1 ObigoInternetBrowser/Q05A",
        "ALCATEL_ONE_TOUCH_815/1.0 Profile/MIDP-2.1 Configuration/CLDC-1.1 ObigoInternetBrowser/Q05A",
        "AndroidDownloadManager/5.0.2 (Linux; U; Android 5.0.2; SM-A500FU Build/LRX22G)",
        "Mozilla/5.0 (iPhone; CPU iPhone OS 10_2_1 like Mac OS X) AppleWebKit/602.4.6 (KHTML, like Gecko) Mobile/14D27 [FBAN/FBIOS;FBAV/73.0.0.44.70;FBBV/44368320;FBRV/0;FBDV/iPhone8,4;FBMD/iPhone;FBSN/iOS;FBSV/10.2.1;FBSS/2;FBCR/Salt;FBID/phone;FBLC/fr_FR;FBOP/5]",
        "Mozilla/5.0 (iPhone; CPU iPhone OS 10_2_1 like Mac OS X) AppleWebKit/602.4.6 (KHTML, like Gecko) Mobile/14D27 [FBAN/FBIOS;FBAV/73.0.0.44.70;FBBV/44368320;FBRV/0;FBDV/iPhone8,4;FBMD/iPhone;FBSN/iOS;FBSV/10.2.1;FBSS/2;FBCR/SaskTel;FBID/phone;FBLC/en_US;FBOP/5]",
        "Mozilla/5.0 (iPhone; CPU iPhone OS 10_2_1 like Mac OS X) AppleWebKit/602.4.6 (KHTML, like Gecko) Mobile/14D27 [FBAN/FBIOS;FBAV/73.0.0.44.70;FBBV/44368320;FBRV/0;FBDV/iPhone8,4;FBMD/iPhone;FBSN/iOS;FBSV/10.2.1;FBSS/2;FBCR/SFR;FBID/phone;FBLC/fr_FR;FBOP/5]",
        "Mozilla/5.0 (iPhone; CPU iPhone OS 10_2_1 like Mac OS X) AppleWebKit/602.4.6 (KHTML, like Gecko) Mobile/14D27 [FBAN/FBIOS;FBAV/73.0.0.44.70;FBBV/44368320;FBRV/0;FBDV/iPhone8,4;FBMD/iPhone;FBSN/iOS;FBSV/10.2.1;FBSS/2;FBCR/Sprint;FBID/phone;FBLC/en_US;FBOP/5]",
        "Mozilla/5.0 (iPhone; CPU iPhone OS 10_2_1 like Mac OS X) AppleWebKit/602.4.6 (KHTML, like Gecko) Mobile/14D27 [FBAN/FBIOS;FBAV/73.0.0.44.70;FBBV/44368320;FBRV/0;FBDV/iPhone8,4;FBMD/iPhone;FBSN/iOS;FBSV/10.2.1;FBSS/2;FBCR/StarHub;FBID/phone;FBLC/en_GB;FBOP/5]",
        "Mozilla/5.0 (iPhone; CPU iPhone OS 9_2_1 like Mac OS X) AppleWebKit/601.1.46 (KHTML, like Gecko) Mobile/13D15 [FBAN/FBIOS;FBAV/76.0.0.45.339;FBBV/47725512;FBRV/0;FBDV/iPhone6,2;FBMD/iPhone;FBSN/iPhone OS;FBSV/9.2.1;FBSS/2;FBCR/TelenorDK;FBID/phone;FBLC/da_DK;FBOP/5]",
        "Mozilla/5.0 (iPhone; CPU iPhone OS 9_2_1 like Mac OS X) AppleWebKit/601.1.46 (KHTML, like Gecko) Mobile/13D15 [FBAN/FBIOS;FBAV/76.0.0.45.339;FBBV/47725512;FBRV/0;FBDV/iPhone6,2;FBMD/iPhone;FBSN/iPhone OS;FBSV/9.2.1;FBSS/2;FBCR/TELIA;FBID/phone;FBLC/da_DK;FBOP/5]",
        "Mozilla/5.0 (iPhone; CPU iPhone OS 9_2_1 like Mac OS X) AppleWebKit/601.1.46 (KHTML, like Gecko) Mobile/13D15 [FBAN/FBIOS;FBAV/76.0.0.45.339;FBBV/47725512;FBRV/0;FBDV/iPhone6,2;FBMD/iPhone;FBSN/iPhone OS;FBSV/9.2.1;FBSS/2;FBCR/Telstra;FBID/phone;FBLC/en_GB;FBOP/5]",
        "Mozilla/5.0 (iPhone; CPU iPhone OS 9_2_1 like Mac OS X) AppleWebKit/601.1.46 (KHTML, like Gecko) Mobile/13D15 [FBAN/FBIOS;FBAV/76.0.0.45.339;FBBV/47725512;FBRV/0;FBDV/iPhone6,2;FBMD/iPhone;FBSN/iPhone OS;FBSV/9.2.1;FBSS/2;FBCR/vodaAU;FBID/phone;FBLC/en_GB;FBOP/5]",
        "Mozilla/5.0 (iPhone; CPU iPhone OS 9_2_1 like Mac OS X) AppleWebKit/601.1.46 (KHTML, like Gecko) Mobile/13D15 [FBAN/FBIOS;FBAV/76.0.0.45.339;FBBV/47725512;FBRV/0;FBDV/iPhone6,2;FBMD/iPhone;FBSN/iPhone OS;FBSV/9.2.1;FBSS/2;FBCR/vodaAU;FBID/phone;FBLC/en_US;FBOP/5]",
        "Mozilla/5.0 (iPhone; CPU iPhone OS 9_2_1 like Mac OS X) AppleWebKit/601.1.46 (KHTML, like Gecko) Mobile/13D15 [FBAN/FBIOS;FBAV/76.0.0.45.339;FBBV/47725512;FBRV/0;FBDV/iPhone6,2;FBMD/iPhone;FBSN/iPhone OS;FBSV/9.2.1;FBSS/2;FBCR/Vodafone.de;FBID/phone;FBLC/en_GB;FBOP/5]",
        "Mozilla/5.0 (iPhone; CPU iPhone OS 9_2_1 like Mac OS X) AppleWebKit/601.1.46 (KHTML, like Gecko) Mobile/13D15 [FBAN/FBIOS;FBAV/76.0.0.45.339;FBBV/47725512;FBRV/0;FBDV/iPhone6,2;FBMD/iPhone;FBSN/iPhone OS;FBSV/9.2.1;FBSS/2;FBCR/vodafoneUK;FBID/phone;FBLC/en_GB;FBOP/5]",
        "Pingdom.com_bot_version_1.4_(http://www.pingdom.com/)",
        "Mozilla/5.0 (X11; Linux x86_64; rv:52.0) Gecko/20100101 Firefox/52.0 DejaClick/2.9.6.0",
        "5.0 (Linux; Android 5.0.1; GEM-701L Build/HUAWEIGEM-701L) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/55.0 Mobile Safari/537",
        "Mozilla/5.0 (Linux; Android 5.0.1; GEM-701L Build/HUAWEIGEM-701L) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/56.0.2924.87 Mobile Safari/537.36",
        "Mozilla/5.0 (Linux; Android 5.0.1; GEM-701L Build/HUAWEIGEM-701L) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/56.0 Mobile Safari/537",
        "Mozilla/5.0 (Linux; Android 5.0.1; GEM-701L Build/HUAWEIGEM-701L) AppleWebKit/537.36 (KHTML, like Gecko) Version/4.0 Chrome/37.0.0.0 Mobile Safari/537.36",
        "Mozilla/5.0 (Linux; Android 5.0.1; GEM-701L Build/HUAWEIGEM-701L) AppleWebKit/537.36 (KHTML, like Gecko) Version/4.0 Chrome/37.0.0.0 Mobile Safari/537.36 ACHEETAHI/1",
        "Mozilla/5.0 (Linux; Android 5.0.1; GEM-701L Build/HUAWEIGEM-701L) AppleWebKit/537.36 (KHTML, like Gecko) Version/4.0 Chrome/37.0.0.0 Mobile Safari/537.36 [FB_IAB/FB4A;FBAV/108.0.0.17.68;]",
        "Mozilla/5.0 (Linux; Android 5.1; LG-X210 Build/LMY47I; wv) AppleWebKit/537.36 (KHTML, like Gecko) Version/4.0 Chrome/49.0.2623.105 Mobile Safari/537.36 [FB_IAB/FB4A;FBAV/107.0.0.19.337;]",
        "Mozilla/5.0 (Linux; Android 5.1; LG-X210 Build/LMY47I; wv) AppleWebKit/537.36 (KHTML, like Gecko) Version/4.0 Chrome/49.0.2623.105 Mobile Safari/537.36 [FB_IAB/FB4A;FBAV/108.0.0.17.68;]",
        "Mozilla/5.0 (Linux; Android 5.1; LG-X210 Build/LMY47I; wv) AppleWebKit/537.36 (KHTML, like Gecko) Version/4.0 Chrome/49.0.2623.105 Mobile Safari/537.36 [FB_IAB/FB4A;FBAV/109.0.0.15.71;]",
        "Mozilla/5.0 (Linux; Android 5.1; LG-X210 Build/LMY47I; wv) AppleWebKit/537.36 (KHTML, like Gecko) Version/4.0 Chrome/49.0.2623.105 Mobile Safari/537.36 [FB_IAB/FB4A;FBAV/110.0.0.15.69;]",
        "Mozilla/5.0 (Linux; Android 5.1; LG-X210 Build/LMY47I; wv) AppleWebKit/537.36 (KHTML, like Gecko) Version/4.0 Chrome/49.0.2623.105 Mobile Safari/537.36 [FB_IAB/FB4A;FBAV/86.0.0.19.69;]",
        "Mozilla/5.0 (Linux; Android 5.1; LG-X210 Build/LMY47I; wv) AppleWebKit/537.36 (KHTML, like Gecko) Version/4.0 Chrome/49.0.2623.105 Mobile Safari/537.36 [FB_IAB/FB4A;FBAV/94.0.0.17.68;]",
        "Mozilla/5.0 (Linux; U; Android 4.1.1; fr-fr; N90FHDRK Build/JRO03H) AppleWebKit/534.30 (KHTML, like Gecko) Version/4.0 Safari/534.30",
        "Mozilla/5.0 (Linux; U; Android 4.1.1; fr-fr; N9588 Build/JRO03C) AppleWebKit/534.30 (KHTML, like Gecko) Version/4.0 Mobile Safari/534.30",
        "Mozilla/5.0 (Linux; U; Android 4.1.1; fr-fr; N9588 Build/JRO03C) AppleWebKit/534.30 (KHTML, like Gecko) Version/4.0 Mobile Safari/534.30 [FB_IAB/FB4A;FBAV/31.0.0.20.13;]",
        "Mozilla/5.0 (Linux; U; Android 4.1.1; fr-fr; Nexus S Build/JRO03E) AppleWebKit/534.30 (KHTML, like Gecko) Version/4.0 Mobile Safari/534.30",
        "Mozilla/5.0 (Linux; U; Android 4.1.1; fr-fr; NOON Build/JRO03H) AppleWebKit/534.30 (KHTML, like Gecko) Version/4.0 Safari/534.30",
        "Mozilla/5.0 (Linux; U; Android 4.1.1; fr-fr; Novo10 Hero Build/20121226) AppleWebKit/534.30 (KHTML, like Gecko) Version/4.0 Safari/534.30",
        "Opera/9.80 (BREW; Opera Mini/6.0.3/27.2414; U; en) Presto/2.8.119 240X400 LG VN360",
        "Opera/9.80 (BREW; Opera Mini/6.0.3/27.2414; U; en) Presto/2.8.119 240X400 Pantech CDM8992",
        "Opera/9.80 (BREW; Opera Mini/6.0.3/27.2414; U; en) Presto/2.8.119 320X240 LG VN530",
        "Opera/9.80 (BREW; Opera Mini/6.0.3/27.2414; U; en) Presto/2.8.119 320X240 Pantech TXT8045",
        "Opera/9.80 (BREW; Opera Mini/6.0.3/27.2414; U; en) Presto/2.8.119 320X240 Samsung SCH-U380",
        "Opera/9.80 (BREW; Opera Mini/6.0.3/27.2414; U; en) Presto/2.8.119 320X240 Samsung SCH-U485",
        "Opera/9.80 (BREW; Opera Mini/6.0.3/27.2414; U; en) Presto/2.8.119 400X240 LG VN280",
        "ZTEU887_TD/1.0 Linux/2.6.35.7 Android/2.3.5 Release/01.01.2012 Browser/AppleWebKit533.1 Mozilla/5.0 Mobile",
        "ZTEU930HD_TD/1.0 Linux/3.0.8 Android/4.0 Release/10.18.2012 Browser/AppleWebKit534.30",
        "ZTE U930_TD/1.0 Linux/2.6.39 Android/4.0 Release/3.5.2012 Browser/AppleWebKit534.30",
        "ZTE U950_TD/1.0 Linux/3.1.10 Android/4.0 Release/8.1.2012 Browser/AppleWebKit534.30",
        "ZTE-U F110/WAP2.0 Profile/MIDP-2.0",
        "ZTE-Vodafone550(Keren)",
        "ZTE-Z222/1.0.6 NetFront/3.5 QTV5.1 Profile/MIDP-2.1 Configuration/CLDC-1.1",
        "ZTE-Z222/1.0.9 NetFront/3.5 QTV5.1 Profile/MIDP-2.1 Configuration/CLDC-1.1",
        "ZTE-Z222(Rogers)/1.8.0 NetFront/3.5 QTV5.1 Profile/MIDP-2.1 Configuration/CLDC-1.1",
        "ZTE-Z223/V1.0.6 NetFront/3.5 QTV5.1 Profile/MIDP-2.1 Configuration/CLDC-1.1",
        "ZTE-Z331/1.5.0 NetFront/3.5 QTV5.1 Profile/MIDP-2.1 Configuration/CLDC-1.1",
        "ZTE-Z431/1.4.0 NetFront/4.2 QTV5.1 Profile/MIDP-2.1 Configuration/CLDC-1.1",
        "ZTE-Z431/1.7.0 NetFront/4.2 QTV5.1 Profile/MIDP-2.1 Configuration/CLDC-1.1",
        "ZTE-Z432/1.0.3 NetFront/4.2 QTV5.1 Profile/MIDP-2.1 Configuration/CLDC-1.1",
        "ZTE-Z432/2.0.11 NetFront/4.2 QTV5.1 Profile/MIDP-2.1 Configuration/CLDC-1.1",
        "ZTE-Z432/2.0.9 NetFront/4.2 QTV5.1 Profile/MIDP-2.1 Configuration/CLDC-1.1",
        "ZTE-Z433/Z433V1.4.0 NetFront/4.2 QTV5.1 Profile/MIDP-2.1 Configuration/CLDC-1.1",
        "ZTE-ZTE-U791(OPEN)/1.0 Linux/2.6.35.7 Android/2.3.5 Release 04.09.2013 Browser/AppleWebKit533.1 (KHTML, like Gecko) Mozilla/5.0 Mobile",
        "Zuriost/2.2.0 (MEDION P1032X; Android 5.0; Code 21) XWalkView/15.44.384.13 Build/10",
        "ZVII/V1 Linux/3.18.19 Android/6.0 Release/06.06.2016 Browser/ AppleWebKit/537.36 Mobile Safari/537.36 System/Android 6.0;",
        "ZVII/V1 Linux/3.18.19 Android/6.0 Release/08.06.2016 Browser/ AppleWebKit/537.36 Mobile Safari/537.36 System/Android 6.0;",
        "ZVII/V1 Linux/3.18.19 Android/6.0 Release/08.30.2016 Browser/ AppleWebKit/537.36 Mobile Safari/537.36 System/Android 6.0;",
        "ZVII/V1 Linux/3.18.19 Android/6.0 Release/??.??.???? Browser/ AppleWebKit/537.36 Mobile Safari/537.36 System/Android 6.0;",
        "ZVI/V1 Linux/3.10.72 Android/5.1 Release/01.13.2016 Browser/ AppleWebKit/537.36 Mobile Safari/537.36 System/Android 5.1;",
        "YouTube/12.05.7 CFNetwork/758.5.3 Darwin/15.6.0",
        "YouTube/12.05.7 CFNetwork/808.2.16 Darwin/16.3.0",
        "YMobile/1.0 FlurrySDK/6.8.0 (com.yahoo.mobile.client.android.yahoo/7.5.0; Android/6.0.1; MMB29M; a5lte; samsung; SM-A500M; 4.97; 1280x720;)",
        "YMobile/1.0 FlurrySDK/6.8.0 (com.yahoo.mobile.client.android.yahoo/7.5.0; Android/6.0.1; MMB29M; trltevzw; samsung; SM-N910V; 5.71; 2560x1440;)",
        "you-android/2.1.0b66 (Dalvik/2.1.0; Linux; U; Android 5.1.1; D5803 Build/23.4.A.1.264)",
        "Youdao Desktop Dict (Windows NT 6.1)",
        "Youku HD;4.1.1;iPhone OS;10.2.1;iPad5,3",
        "Youku HD;4.2.1;iPhone OS;9.2.1;iPad5,3",
        "Xiaomi_2015562_TD-LTE/V1 Linux/3.10.49 Android/5.1.1 Release/9.9.2015 Browser/AppleWebKit537.36 Mobile Safari/537.36 System/Android 5.1.1 XiaoMi/MiuiBrowser/2.4.0",
        "Xiaomi_2015611_TD-LTE/V1 Linux/3.10.61 Android/5.0 Release/11.11.2015 Browser/AppleWebKit537.36 Mobile Safari/537.36 System/Android 5.0 XiaoMi/MiuiBrowser/2.4.9",
        "yacybot (/global; amd64 Linux 3.16.0-4-amd64; java 1.8.0_121; Etc/en) http://yacy.net/bot.html",
        "randomic_undetectable_string",
        "WordPress/4.7; http://digrit.com",
        "WordPress/4.7; http://turistigid.com",
        "WordPress/4.7; http://www.kunstleben-berlin.de",
        "WordPress/5226; http://www.stadtbekannt.at",
        "WordPress/5582; http://www.stadtbekannt.at",
        "WordPress/9220; http://www.stadtbekannt.at",
        "WordPress.com; http://clarkdeals.com",
        "WordPress.com; http://saetzeundschaetze.com",
        "WordPress.com; https://apodyopsissite.wordpress.com",
        "WordPress.com; https://doktorungezirehberi.wordpress.com",
        "WordPress.com; https://fullmoonatnoon.wordpress.com",
        "WordPress.com; https://honors174winter2017.wordpress.com",
        "MyApp/1.0"
        };

        public static String[] CreateTestUserAgentList()
        {
            List<String> userAgents = new List<String>(10000);
            for (int i = 0; i < 100; i++)
            {
                userAgents.AddRange(UserAgents);
            }
            return userAgents.ToArray();
        }

    }
}