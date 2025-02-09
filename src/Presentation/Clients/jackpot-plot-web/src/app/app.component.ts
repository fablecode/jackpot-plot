import {AfterViewInit, Component, HostBinding} from '@angular/core';
import { RouterOutlet } from '@angular/router';
import KTComponents from '../metronic/core/index';
import KTLayout from '../metronic/app/layouts/demo1';
import {HeaderComponent} from './layouts/header/header.component';
import {FooterComponent} from './layouts/footer/footer.component';
import {SidebarComponent} from './layouts/sidebar/sidebar.component';
import {SearchModalComponent} from './partials/search-modal/search-modal.component';


@Component({
  selector: 'app-root',
  standalone: true,
  imports: [RouterOutlet, HeaderComponent, FooterComponent, SidebarComponent, SearchModalComponent],
  templateUrl: './app.component.html',
  styleUrl: './app.component.scss'
})
export class AppComponent implements AfterViewInit {
  title = 'jackpot-plot-web';
  @HostBinding('class') hostClass = 'flex grow';

  ngAfterViewInit(): void {
    KTComponents.init();
    KTLayout.init();
  }
}
