
using Azure;
using System.IO;

namespace PageObjects
{
    public class PageObjects
    {
        public static string LabelAmazonPriceInteger = "#apex_desktop .a-price-whole";
        public static string LabelAmazonPriceDecimal = "#apex_desktop .a-price-fraction";
        public static string LabelAmazonProductName = "[class='a-size-large product-title-word-break']";

        public static string LabelKabumPrice = ".finalPrice";
        public static string LabelKabumProductName = "xpath=//div[@id='container-purchase']/div/div/h1";

        public static string LabelPlaystationPrice = "[data-qa='mfeCtaMain#offer0#finalPrice']";
        public static string LabelPlaystationProductName = "[data-qa='mfe-game-title#name']";

        public static string LabelPichauErro404 = "text=Página não encontrada";
        public static string LabelPichauPrice = "//span[text()='à vista']";
        public static string LabelPichauProductName = "[data-cy='product-page-title']";

        public static string LabelMagazineLuizaPrice = "[data-testid=showcase-price]";
        public static string LabelMagazineLuizaProductName = "[data-testid=heading-product-title]";
        public static string LabelMagazineLuizaProdutoNaoDisponivel = "[data-testid=let-me-know-title]";

        public static string LabelTerabyteProductName = "[class=tit-prod]";
        public static string LabelTerabytePrice = "[id=valVista] >> nth = 0";

        public static string LabelNuuvemProductName = "[class=product-title] >> nth = 0";
        public static string LabelNuuvemPriceInteger = "[class=integer] >> nth = 0";
        public static string LabelNuuvemPriceDecimal = "[class=decimal] >> nth = 0";

        public static string LabelGreenManGamingProductName = "[ng-bind=\"product.name\"]";
        public static string LabelGreenManGamingPrice = "[class='current-price pdp-price']";

        public static string LabelGOGProductName = "[selenium-id=\"ProductTitle\"]";
        public static string LabelGOGPrice = "[selenium-id=\"ProductFinalPrice\"]";

        public static string ScriptEpic = "script#_schemaOrgMarkup-Product";

        public static string LabelXboxProductName = "data-testid=ProductDetailsHeaderProductTitle";
        public static string LabelXboxPrice = "[class*='AcquisitionButtons-module__listedPrice']";
    }
}
