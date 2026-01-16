import { Component } from "@angular/core";
import { ButtonModule } from "primeng/button";
import { TableModule } from "primeng/table";

@Component({
  selector: "cp-documents-list",
  standalone: true,
  imports: [TableModule, ButtonModule],
  templateUrl: "./documents-list.html",
})
export class DocumentsList {}
