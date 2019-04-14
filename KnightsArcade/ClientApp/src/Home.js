import React, { Component } from 'react';
import './App.css';
import NaviBar from './Components/NavBar';
import { Jumbotron, Button } from 'react-bootstrap';
import GameSlides from './Components/GameSlides';
import Amplify from 'aws-amplify';
import awsmobile from './aws-exports';
import Footer from './Components/Footer';
Amplify.configure(awsmobile);



class Home extends Component {

  render() {
      return (
        <div>
        <div className="App">
        <NaviBar/>
        <Jumbotron className ="Jumbo">
          <h1 className = "text"> Knights Arcade</h1>
          <p className = "text">
            Play / Share / Showcase
          </p>
          <p><a href='/Locations'>
            <Button bsStyle="primary">Find An Arcade Machine</Button>
          </a></p>
        </Jumbotron>

        <GameSlides/>
              </div>
              <Footer scrolls={false} />
              </div>
    )
  }
}

export default Home;