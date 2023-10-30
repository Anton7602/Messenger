namespace MessengerServer.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class CreateMessages : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Messages",
                c => new
                    {
                        ID = c.Int(nullable: false, identity: true),
                        SenderName = c.String(),
                        Time = c.DateTime(nullable: false),
                        Text = c.String(),
                    })
                .PrimaryKey(t => t.ID);
        }
        
        public override void Down()
        {
            DropTable("dbo.Messages");
        }
    }
}
