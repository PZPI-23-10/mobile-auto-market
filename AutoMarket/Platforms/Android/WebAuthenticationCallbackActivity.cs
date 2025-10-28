using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Microsoft.Maui.Authentication;

namespace AutoMarket.Platforms.Android
{
    [Activity(NoHistory = true, LaunchMode = LaunchMode.SingleTop, Exported = true)]
    // ✅ ЦЕЙ ФІЛЬТР "ЛОВИТЬ" ТВОЮ СХЕМУ URI
    [IntentFilter(new[] { Intent.ActionView },
              Categories = new[] { Intent.CategoryDefault, Intent.CategoryBrowsable },
              // ❗ ЗАМІНИ "automarketapp" НА ТВОЮ СХЕМУ
              DataScheme = "com.companyname.automarket")]
    public class WebAuthenticationCallbackActivity : WebAuthenticatorCallbackActivity
    {
        // Цей клас залишається порожнім, вся магія в атрибутах [Activity] та [IntentFilter]
    }
}
