namespace Vadronia
{
    public struct VillageBuilding
    {
        public readonly int Choice;
        public readonly string Name, Function, Text;
        public readonly FootPoint Door;
        public VillageBuilding(int choice,string name,string function,float x,float y,string text)
        {Choice=choice;Name=name;Function=function;Door=new FootPoint(x,y);Text=text;}
    }
    // Doors are south of the footprint, never at the centre of the building sprite.
    public static class VillageBuildings
    {
        public static readonly VillageBuilding[] All = {
            new VillageBuilding(6,"Guilda de Grünwald","Registros e notícias",-3,4.42f,GrunwaldStory.GuildaText),
            new VillageBuilding(20,"Prefeitura de Grünwald","Administração da vila",4.3f,4.42f,
                "A bandeira azul marca a prefeitura. Os comunicados públicos ficam no mural da praça; registros de viajantes e notícias da estrada são tratados na guilda, ao lado."),
            new VillageBuilding(5,"Estalagem de Helga","Descanso e abrigo",12.3f,5.62f,
                "A estalagem acolhe viajantes. Descanse à entrada para recuperar o fôlego ou converse com Helga."),
            new VillageBuilding(21,"Ferraria de Bruno","Oficina e treino",9,1.82f,
                "A forja de Bruno atende os moradores e viajantes. Converse com o ferreiro para ouvir dicas e pratique com o boneco na praça. A oficina ainda não oferece compra de equipamento."),
            new VillageBuilding(30,"Casa de Lúcia","Moradia · jardim norte",-7.4f,5.42f,"Esta é a casa de Lúcia. Procure a moradora junto ao jardim a noroeste."),
            new VillageBuilding(31,"Casa do pátio oeste","Moradia",-8,-.08f,"Uma residência do pátio oeste. A passagem pela frente leva à praça e ao mercado."),
            new VillageBuilding(32,"Casa da rua oeste","Moradia",-11.8f,.12f,"Residência da rua oeste. O jardim e os caminhos ao redor fazem parte do bairro dos moradores."),
            new VillageBuilding(33,"Casa junto à forja","Moradia",12.4f,.02f,"Uma residência ao lado da oficina de Bruno. A entrada fica voltada para a rua ao sul."),
            new VillageBuilding(34,"Casa de Tomás","Moradia · rua sul",-8.2f,-7.88f,"Esta é a casa de Tomás. Ele costuma caminhar pela rua ao sul da vila."),
            new VillageBuilding(35,"Casa dos pinheiros","Moradia",-12,-7.98f,"Residência da rua dos pinheiros. Siga a viela para chegar ao centro da vila."),
            new VillageBuilding(36,"Casa da rua sul","Moradia",3.2f,-7.88f,"Residência próxima à entrada sul. O caminho à frente contorna a praça."),
            new VillageBuilding(37,"Casa do bosque leste","Moradia",11.9f,-7.98f,"Residência à margem do bosque. A rua ao sul liga este quintal ao restante da vila.")
        };
    }
}
