using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Visual_Studio_Solutions.PSDUnityEditor.MyPsdImporter
{
    public class LayerImportFactory
    {
        static LayerImportFactory instance;
        public static LayerImportFactory Instance
        {
            get
            {
                if (instance == null)
                    instance = new LayerImportFactory();
                return instance;
            }
        }

        TextImport textImporter;
        ImageImport imageImporter;
        EmptyImport emptyImporter;

        TextImport TextImporter
        {
            get
            {
                if (textImporter == null)
                    textImporter = new TextImport();
                return textImporter;
            }
        }

        ImportBase ImageImporter
        {
            get
            {
                if (imageImporter == null)
                    imageImporter = new ImageImport();
                return imageImporter;
            }
        }

        EmptyImport EmptyImporter
        {
            get 
            {
                if (emptyImporter == null)
                    emptyImporter = new EmptyImport();
                return emptyImporter;
            }
        }

        public ImportBase GetLayerImporter(PsdLayerNode node)
        {
            var image = node.image;
            if (image != null)
            {
                if (image.type == ImgType.Label)
                    return TextImporter;
                else
                    return ImageImporter;
            }else
            {
                return EmptyImporter;
            }
        }
    }
}
