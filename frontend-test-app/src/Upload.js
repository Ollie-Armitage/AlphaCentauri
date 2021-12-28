import {Component} from "react";
import axios from 'axios';


class Upload extends Component {
     state = {
         selectedFile: null
     }

     onFileChange = event => {
         this.setState({selectedFile: event.target.files[0]});
     }

     onFileUpload = () => {
         const formData = new FormData();
         formData.append("uploadedFile", this.state.selectedFile, this.state.selectedFile.name);

         axios.post("http://localhost:5000/Upload", formData).then(r => console.log(r))
     }

    render() {
        return (
            <div>
                <input type="file" name="file" onChange={this.onFileChange} />
                <div>
                    <button onClick={this.onFileUpload}>Submit</button>
                </div>
            </div>
        )
    }
}

export default Upload